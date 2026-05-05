using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Hosting.Internal
{
    internal class SpectreConsoleHostedService(
        IHostApplicationLifetime applicationLifetime,
        IEnumerable<ExecuteCommandAppDelegate> commandAppExecuteDelegates,
        ILogger<SpectreConsoleHostedService> logger)
        : IHostedService
    {
        private int _exitCode;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = RunCommandAppsAsync(cancellationToken);

            logger.LogDebug("Spectre Console Hosted service is started.");

            return Task.CompletedTask;
        }

        private async Task RunCommandAppsAsync(CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Task> tasks =
                    commandAppExecuteDelegates.Select((del) => RunCommandAppAsync(del, cancellationToken));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                applicationLifetime.StopApplication();
            }
        }

        private async Task RunCommandAppAsync(ExecuteCommandAppDelegate executeCommandAppDelegate, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug("Running command app");
                _exitCode = await executeCommandAppDelegate(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled exception");
                _exitCode = -1;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Setting exit code: {exitCode}", _exitCode);

            Environment.ExitCode = _exitCode;

            return Task.CompletedTask;
        }
    }
}
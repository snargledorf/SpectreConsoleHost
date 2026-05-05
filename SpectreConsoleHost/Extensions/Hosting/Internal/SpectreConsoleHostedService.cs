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
        private readonly TaskCompletionSource<int> _commandsFinishedEvent = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = RunCommandAppsAsync();

            logger.LogDebug("Spectre Console Hosted service is started.");

            return Task.CompletedTask;
        }

        private async Task RunCommandAppsAsync()
        {
            try
            {
                IEnumerable<Task<int>> tasks = commandAppExecuteDelegates.Select(RunCommandAppAsync);

                IEnumerable<int> exitCodes = await Task.WhenAll(tasks).ConfigureAwait(false);

                _commandsFinishedEvent.TrySetResult(exitCodes.Last());
            }
            finally
            {
                applicationLifetime.StopApplication();
            }
        }

        private async Task<int> RunCommandAppAsync(ExecuteCommandAppDelegate executeCommandAppDelegate)
        {
            try
            {
                logger.LogDebug("Running command app");
                return await executeCommandAppDelegate(applicationLifetime.ApplicationStopping).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled exception");
                return -1;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            int exitCode =  await _commandsFinishedEvent.Task;
            logger.LogDebug("Setting exit code: {exitCode}", exitCode);
            Environment.ExitCode = exitCode;
        }
    }
}
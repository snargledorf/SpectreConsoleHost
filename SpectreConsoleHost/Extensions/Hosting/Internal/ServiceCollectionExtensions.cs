using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Internal
{
    internal static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddSpectreConsole(this IServiceCollection services, IEnumerable<string> args,
            Func<ITypeRegistrar, ICommandApp> commandAppFactory, Action<IConfigurator> configureApp = null)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            IServiceProvider serviceProvider = null;
            var tr = new SpectreConsoleHostTypeRegistrar(() => serviceProvider ?? throw new InvalidOperationException("ServiceProvider not built"));
            
            ICommandApp commandApp = commandAppFactory(tr);
            
            return services.AddSpectreConsole(args, commandApp, configureApp, sp => serviceProvider = sp);
        }
        
        internal static IServiceCollection AddSpectreConsole(this IServiceCollection services,
            IEnumerable<string> args, ICommandApp commandApp, Action<IConfigurator> configure, Action<IServiceProvider> provideServiceProvider = null)
        {
            if (commandApp is null)
                throw new ArgumentNullException(nameof(commandApp));
            
            if (args is null)
                throw new ArgumentNullException(nameof(args));
            
            services.AddHostedService<SpectreConsoleHostedService>();

            services.AddSingleton<ExecuteCommandAppDelegate>(sp =>
            {
                provideServiceProvider?.Invoke(sp);
                
                return cancellationToken =>
                {
                    if (configure != null)
                        commandApp.Configure(configure);
                    
                    return commandApp.RunAsync(args, cancellationToken);
                };
            });
            
            return services;
        }
    }
}
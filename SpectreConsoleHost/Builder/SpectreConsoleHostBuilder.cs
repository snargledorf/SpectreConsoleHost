using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console.Builder.Internal;
using Spectre.Console.Cli;
using Spectre.Console.Extensions.Hosting.Internal;

namespace Spectre.Console.Builder
{
    internal class SpectreConsoleHostBuilder<TDefaultCommand>(params string[] args)
        : SpectreConsoleHostBuilder(args, tr => new CommandApp<TDefaultCommand>(tr))
        where TDefaultCommand : class, ICommand;

    public class SpectreConsoleHostBuilder : IHostApplicationBuilder
    {
        private readonly HostApplicationBuilder _builder;

        private IHost _builtHost;
    
        private readonly SpectreConsoleHostConfigurator _configurator;

        internal SpectreConsoleHostBuilder(string[] args, Func<ITypeRegistrar, ICommandApp> buildCommandApp = null)
        {
            _builder = Host.CreateApplicationBuilder(args);
            
            var typeRegistrar = new SpectreConsoleHostTypeRegistrar(() =>
            {
                IHost builtHost = _builtHost ?? throw new InvalidOperationException("Host has not been built");
                return builtHost.Services;
            });
            
            _configurator = new SpectreConsoleHostConfigurator(typeRegistrar);
            
            ICommandApp commandApp = buildCommandApp?.Invoke(typeRegistrar) ?? new CommandApp(typeRegistrar);

            Services.AddSpectreConsole(args, commandApp, appConfigurator => _configurator.Configure(appConfigurator));
        }

        void IHostApplicationBuilder.ConfigureContainer<TContainerBuilder>(
            IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder> configure) =>
            _builder.ConfigureContainer(factory, configure);

        IDictionary<object, object> IHostApplicationBuilder.Properties => ((IHostApplicationBuilder)_builder).Properties;

        public ConfigurationManager Configuration => _builder.Configuration;
        IConfigurationManager IHostApplicationBuilder.Configuration => Configuration;

        public IHostEnvironment Environment => _builder.Environment;

        public ILoggingBuilder Logging => _builder.Logging;

        public IMetricsBuilder Metrics => _builder.Metrics;

        public IServiceCollection Services => _builder.Services;

        public IConfigurator Configurator => _configurator;

        public IHost Build()
        {
            return _builtHost = _builder.Build();
        }
    }
}
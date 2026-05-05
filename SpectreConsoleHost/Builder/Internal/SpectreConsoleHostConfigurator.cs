using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;

namespace Spectre.Console.Builder.Internal
{
    internal class SpectreConsoleHostConfigurator(ITypeRegistrar typeRegistrar) : IConfigurator
    {
        private readonly List<Action<IConfigurator>> _configureActions = new List<Action<IConfigurator>>();

        public IConfigurator SetHelpProvider(IHelpProvider helpProvider)
        {
            _configureActions.Add(configurator => configurator.SetHelpProvider(helpProvider));
            return this;
        }

        public IConfigurator SetHelpProvider<T>() where T : IHelpProvider
        {
            _configureActions.Add(configurator => configurator.SetHelpProvider<T>());
            return this;
        }

        public IConfigurator AddExample(params string[] args)
        {
            _configureActions.Add(configurator => configurator.AddExample(args));
            return this;
        }

        public ICommandConfigurator AddCommand<TCommand>(string name) where TCommand : class, ICommand
        {
            var addCommandConfigurator = new SpectreConsoleHostCommandConfigurator();

            _configureActions.Add(configurator => addCommandConfigurator.Configure(configurator.AddCommand<TCommand>(name)));
        
            return addCommandConfigurator;
        }

        public ICommandConfigurator AddDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, int> func) where TSettings : CommandSettings
        {
            var addDelegateConfigurator = new SpectreConsoleHostCommandConfigurator();
        
            _configureActions.Add(configurator => addDelegateConfigurator.Configure(configurator.AddDelegate(name, func)));
        
            return addDelegateConfigurator;
        }

        public ICommandConfigurator AddAsyncDelegate<TSettings>(string name, Func<CommandContext, TSettings, CancellationToken, Task<int>> func) where TSettings : CommandSettings
        {
            var addAsyncDelegateConfigurator = new SpectreConsoleHostCommandConfigurator();
        
            _configureActions.Add(configurator => addAsyncDelegateConfigurator.Configure(configurator.AddAsyncDelegate(name, func)));
        
            return addAsyncDelegateConfigurator;
        }

        public IBranchConfigurator AddBranch<TSettings>(string name, Action<IConfigurator<TSettings>> action) where TSettings : CommandSettings
        {
            var addBranchConfigurator = new SpectreConsoleHostBranchConfigurator();
        
            _configureActions.Add(configurator => addBranchConfigurator.Configure(configurator.AddBranch(name, action)));
        
            return addBranchConfigurator;
        }

        public ICommandAppSettings Settings { get; } = new SpectreConsoleHostCommandAppSettings(typeRegistrar);

        public void Configure(IConfigurator configurator)
        {
            foreach (Action<IConfigurator> configureAction in _configureActions) 
                configureAction(configurator);
        
            CopySettings(Settings, configurator.Settings);
        }

        private static void CopySettings(ICommandAppSettings from, ICommandAppSettings to)
        {
            to.ApplicationName = from.ApplicationName;
            to.ApplicationVersion = from.ApplicationVersion;
            to.CaseSensitivity = from.CaseSensitivity;
            to.Console = from.Console;
            to.ConvertFlagsToRemainingArguments = from.ConvertFlagsToRemainingArguments;
            to.Culture = from.Culture;
            to.ExceptionHandler = from.ExceptionHandler;
            to.HelpProviderStyles = from.HelpProviderStyles;
            to.MaximumIndirectExamples = from.MaximumIndirectExamples;
            to.PropagateExceptions = from.PropagateExceptions;
            to.ShowOptionDefaultValues = from.ShowOptionDefaultValues;
            to.StrictParsing = from.StrictParsing;
            to.TrimTrailingPeriod = from.TrimTrailingPeriod;
            to.ValidateExamples = from.ValidateExamples;
#pragma warning disable CS0618 // Type or member is obsolete
            to.Interceptor = from.Interceptor;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Internal
{
    internal class SpectreConsoleHostTypeRegistrar(Func<IServiceProvider> getServiceProvider) : ITypeRegistrar
    {
        private readonly Dictionary<Type, List<Func<IServiceProvider, object>>> _factories = new Dictionary<Type, List<Func<IServiceProvider, object>>>();

        public void Register(Type service, Type implementation)
        {
            GetFactoryList(service).Add(sp =>
                ActivatorUtilities.GetServiceOrCreateInstance(sp, implementation));
        }

        public void RegisterInstance(Type service, object implementation)
        {
            GetFactoryList(service).Add(_ => implementation);
        }

        public void RegisterLazy(Type service, Func<object> factory)
        {
            GetFactoryList(service).Add(_ => factory());   
        }

        public ITypeResolver Build()
        {
            return new SpectreConsoleHostTypeResolver(
                _factories.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsReadOnly()).AsReadOnly(),
                getServiceProvider());
        }

        private List<Func<IServiceProvider, object>> GetFactoryList(Type service)
        {
            List<Func<IServiceProvider, object>> factories = _factories.GetValueOrDefault(service);
            if (factories is null)
                _factories[service] = factories = new List<Func<IServiceProvider, object>>();
            return factories;
        }
    }
}
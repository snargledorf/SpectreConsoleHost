using System;
using System.Collections.ObjectModel;
using System.Linq;
using Spectre.Console.Cli;

namespace Spectre.Console.Extensions.Hosting.Internal
{
    internal class SpectreConsoleHostTypeResolver(
        ReadOnlyDictionary<Type, ReadOnlyCollection<Func<IServiceProvider, object>>> serviceFactories,
        IServiceProvider serviceProvider)
        : ITypeResolver
    {
        public object Resolve(Type type)
        {
            if (type is null)
                return null;

            Func<IServiceProvider, object> typeFactory = serviceFactories.GetValueOrDefault(type)?.LastOrDefault();
            return typeFactory is null ? serviceProvider.GetService(type) : typeFactory(serviceProvider);
        }
    }
}
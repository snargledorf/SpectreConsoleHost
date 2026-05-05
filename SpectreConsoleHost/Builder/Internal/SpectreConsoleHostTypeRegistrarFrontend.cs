using System;
using Spectre.Console.Cli;

namespace Spectre.Console.Builder.Internal
{
    internal class SpectreConsoleHostTypeRegistrarFrontend(ITypeRegistrar typeRegistrar) : ITypeRegistrarFrontend
    {
        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            typeRegistrar.Register(typeof(TService), typeof(TImplementation));
        }

        public void RegisterInstance<TImplementation>(TImplementation instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
        
            typeRegistrar.RegisterInstance(typeof(TImplementation), instance);
        }

        public void RegisterInstance<TService, TImplementation>(TImplementation instance) where TImplementation : TService
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
        
            typeRegistrar.RegisterInstance(typeof(TService), instance);
        }
    }
}
using Spectre.Console.Cli;

namespace Spectre.Console.Builder
{
    public static class SpectreConsoleHost
    {
        public static SpectreConsoleHostBuilder CreateBuilder<TDefaultCommand>(params string[] args)
            where TDefaultCommand : class, ICommand
        {
            return new SpectreConsoleHostBuilder<TDefaultCommand>(args);
        }

        public static SpectreConsoleHostBuilder CreateBuilder(params string[] args) => new(args);
    }
}
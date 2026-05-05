using System.Threading;
using System.Threading.Tasks;

namespace Spectre.Console.Extensions.Hosting.Internal
{
    internal delegate Task<int> ExecuteCommandAppDelegate(CancellationToken cancellationToken);
}
using System.Threading;
using System.Threading.Tasks;

namespace SclXplatDnx
{
    internal interface ICommand
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace Ums.Shell.Bootstrapper.Interface
{
    public interface IBootstrapperAsync<out T> : IBootstrapperAsync
    {
        T? Result { get; }
    }

    public interface IBootstrapperAsync
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}

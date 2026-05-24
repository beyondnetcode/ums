using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ums.Shell.Bootstrapper.Interface;

namespace Ums.Shell.Bootstrapper.Impl
{
    public class CompositeBootstrapperAsync : IBootstrapperAsync
    {
        private readonly List<IBootstrapperAsync> _bootstrappers;

        public CompositeBootstrapperAsync(IEnumerable<IBootstrapperAsync> bootstrappers)
        {
            _bootstrappers = new List<IBootstrapperAsync>(bootstrappers);
        }

        public CompositeBootstrapperAsync()
        {
            _bootstrappers = new List<IBootstrapperAsync>();
        }

        public CompositeBootstrapperAsync Add(IBootstrapperAsync bootstrapper)
        {
            _bootstrappers.Add(bootstrapper);
            return this;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            foreach (var bootstrapper in _bootstrappers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await bootstrapper.RunAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

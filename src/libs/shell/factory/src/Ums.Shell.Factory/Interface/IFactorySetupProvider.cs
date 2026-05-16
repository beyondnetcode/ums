using Ums.Shell.Factory.Model;

namespace Ums.Shell.Factory.Interfaces
{
    public interface IFactorySetupProvider
    {
        IEnumerable<IFactorySetupSource> Sources { get; }

        Setup Configuration { get; }

        SetupItem[] Provide<TTarget, TService>(TTarget target, string name);
    }
}

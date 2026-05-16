using Ums.Shell.Factory.Fluent.Interfaces;
using Ums.Shell.Factory.Model;

namespace Ums.Shell.Factory.Fluent.Impl
{
    public class FactorySetupCreateBuilder<TTarget, TService> : IFactoryRecordSetupCreateBuilder<TTarget, TService>
    {
        private readonly SetupItem _item;

        public FactorySetupCreateBuilder(SetupItem item)
        {
            _item = item;
        }

        public IFactoryRecordSetupWhenBuilder<TTarget> Create<TImplementation>() where TImplementation : TService
        {
            _item.ImplementationType = typeof(TImplementation);

            return new FactorySetupWhenBuilder<TTarget>(_item);
        }

    }
}

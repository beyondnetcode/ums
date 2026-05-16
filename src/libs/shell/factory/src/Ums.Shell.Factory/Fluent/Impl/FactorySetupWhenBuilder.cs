using Ums.Shell.Factory.Fluent.Interfaces;
using Ums.Shell.Factory.Model;

namespace Ums.Shell.Factory
{
    public class FactorySetupWhenBuilder<TTarget> : IFactoryRecordSetupWhenBuilder<TTarget>
    {
        private readonly SetupItem _item;

        public FactorySetupWhenBuilder(SetupItem item)
        {
            _item = item;
        }

        public void When(Func<TTarget, bool> selector)
        {
            _item.Selector = selector;
        }
    }
}

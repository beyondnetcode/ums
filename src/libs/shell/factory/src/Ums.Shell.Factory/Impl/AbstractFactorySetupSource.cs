using Ums.Shell.Factory.Interfaces;
using Ums.Shell.Factory.Fluent.Impl;
using Ums.Shell.Factory.Model;
using Ums.Shell.Factory.Fluent.Interfaces;

namespace Ums.Shell.Factory.Impl
{
    public abstract class AbstractFactorySetupSource : IFactorySetupSource
    {
        protected readonly List<SetupItem> Items = new List<SetupItem>();

        public Setup Source()
        {
            var result = new Setup();

            foreach (var item in Items)
            {
                result.Items.Add(item);
            }

            return result;
        }

        public IFactoryRecordSetupCreateBuilder<TTarget, TService> For<TTarget, TService>()
        {
            var value = new SetupItem(typeof(TTarget), typeof(TService), string.Empty);

            var descriptor = new FactorySetupCreateBuilder<TTarget, TService>(value);

            Items.Add(value);

            return descriptor;
        }

        public void For<TTarget, TService>(string name, Action<IFactoryRecordSetupGroupCreateBuilder<TTarget, TService>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var descriptor = new FactorySetupGroupCreateBuilder<TTarget, TService>(Items, name);

            action.Invoke(descriptor);
        }
    }     
}

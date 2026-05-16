using Ums.Shell.Factory.Fluent.Interfaces;
using Ums.Shell.Factory.Model;

namespace Ums.Shell.Factory
{
    public class FactorySetupGroupCreateBuilder<TTarget, TService> : IFactoryRecordSetupGroupCreateBuilder<TTarget, TService>
    {
        private readonly List<SetupItem> _objectFactoryConfigurationItems;

        private readonly string _name;

        public FactorySetupGroupCreateBuilder(List<SetupItem> objectFactoryConfigurationItems, string name)
        {
            _objectFactoryConfigurationItems = objectFactoryConfigurationItems;

            _name = name;
        }

        public IFactoryRecordSetupWhenBuilder<TTarget> Create<TImplementation>() where TImplementation : TService
        {
            var value = new SetupItem(typeof(TTarget), typeof(TImplementation), typeof(TService), _name);

            _objectFactoryConfigurationItems.Add(value);

            return new FactorySetupWhenBuilder<TTarget>(value);
        }

    }
}
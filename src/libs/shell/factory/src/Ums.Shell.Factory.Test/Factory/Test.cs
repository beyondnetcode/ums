using Ums.Shell.Factory.Test.Impl;
using Ums.Shell.Factory.Test.Interfaces;
using Ums.Shell.Factory.Impl;
using Ums.Shell.Factory.Interfaces;


namespace Ums.Shell.Factory.Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Create_WithCustomerOlderThan25_ShouldBeNotEmpty()
        {
            var tests = new TestCases();

            var locator = new ServiceLocator();

            locator.Register(typeof(IDoSomething), new DoSomething(), typeof(DoSomething).FullName);

            var config = new FactoryRecordSetupSource();

            var factory = new Ums.Shell.Factory.Impl.Factory (new FactorySetupProvider(new IFactorySetupSource[] { (IFactorySetupSource)config }), new FactoryCreator(locator));

            tests.CreateWithConsultantOlderThan25ShouldBeNotEmpty(factory);
        }
    }
}

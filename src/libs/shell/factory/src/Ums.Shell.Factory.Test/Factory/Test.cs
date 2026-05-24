using Ums.Shell.Factory.Test.Impl;
using Ums.Shell.Factory.Test.Interfaces;
using Ums.Shell.Factory.Impl;
using Ums.Shell.Factory.Interfaces;

namespace Ums.Shell.Factory.Test
{
    [TestClass]
    public class Test
    {
        /// <summary>
        /// Creates a <see cref="FactoryCreator"/> whose instance factory uses
        /// <see cref="Activator.CreateInstance(Type)"/>.
        ///
        /// This removes all dependency on the (now-deleted) ServiceLocator:
        /// concrete types are resolved by simple reflection, which works for
        /// any type with a public parameterless constructor — including all
        /// factory implementation types in these tests.
        /// </summary>
        private static IFactoryCreator BuildActivatorCreator()
            => new FactoryCreator(type =>
                Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Could not create instance of '{type.FullName}'."));

        [TestMethod]
        public void Create_WithConsultantOlderThan25_ShouldBeNotEmpty()
        {
            var tests = new TestCases();

            var factory = new Factory.Impl.Factory(
                new FactorySetupProvider(new IFactorySetupSource[] { new FactoryRecordSetupSource() }),
                BuildActivatorCreator());

            tests.CreateWithConsultantOlderThan25ShouldBeNotEmpty(factory);
        }

        [TestMethod]
        public void Create_WithConsultantLessThan18_ShouldBeNotEmpty()
        {
            var tests = new TestCases();

            var factory = new Factory.Impl.Factory(
                new FactorySetupProvider(new IFactorySetupSource[] { new FactoryRecordSetupSource() }),
                BuildActivatorCreator());

            tests.CreateWithConsultantLessThan18_ShouldBeNotEmpty(factory);
        }
    }
}

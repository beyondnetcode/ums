using Ums.Shell.Factory.Interfaces;
using Ums.Shell.Factory.Test.Interfaces;
using Ums.Shell.Factory.Test.Models;
using Shouldly;

namespace Ums.Shell.Factory.Test.Impl
{
    public class TestCases
    {
        public void CreateWithConsultantOlderThan25ShouldBeNotEmpty(IFactory factory)
        {
            var customer = new Consultant() { Age = 25 };

            var services = factory.Create<Consultant, IDoSomething>(customer);

            services.ShouldNotBeEmpty();

            services.Length.ShouldBe(1);

            services[0].ShouldBeAssignableTo<IDoSomething>();
            services[0].ShouldBeOfType<DoSomething>();
        }

        public void CreateWithConsultantLessThan18_ShouldBeNotEmpty(IFactory factory)
        {
            var customer = new Consultant() { Age = 15 };

            var services = factory.Create<Consultant, IDoSomething>(customer);

            services.ShouldNotBeEmpty();

            services.Length.ShouldBe(1);

            services[0].ShouldBeAssignableTo<IDoSomething>();

            services[0].ShouldBeOfType<DoSomethingLessThan18>();
        }
    }
}

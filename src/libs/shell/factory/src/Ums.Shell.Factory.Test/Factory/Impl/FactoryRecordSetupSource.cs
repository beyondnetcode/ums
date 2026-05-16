using Ums.Shell.Factory.Impl;
using Ums.Shell.Factory.Test.Interfaces;
using Ums.Shell.Factory.Test.Models;

namespace Ums.Shell.Factory.Test.Impl
{
    public class FactoryRecordSetupSource : AbstractFactorySetupSource
    {
        public FactoryRecordSetupSource()
        {
            For<Consultant, IDoSomething>().Create<DoSomething>().When(x => x.Age > 18);
            For<Consultant, IDoSomething>().Create<DoSomethingLessThan18>().When(x => x.Age < 18);
        }
    }

}


using Ums.Shell.Factory.Demo.Impl;
using Ums.Shell.Factory.Demo.Interfaces;
using Ums.Shell.Factory.Demo.Models;
using Ums.Shell.Factory.Impl;

namespace Ums.Shell.Factory.Demo.Bootstrapper
{
    public class LogicModelFactoryRecordSetup : AbstractFactorySetupSource
    {
        public LogicModelFactoryRecordSetup()
        {
            For<Criteria, ILogicModelLoader>().Create<LogicModelCATLoader>().When(x => x.PersonalizationType == ePersonalizationType.CAT);
            For<Criteria, ILogicModelLoader>().Create<LogicModelPADLoader>().When(x => x.PersonalizationType == ePersonalizationType.PAD);
            For<Criteria, ILogicModelLoader>().Create<LogicModelREVLoader>().When(x => x.PersonalizationType == ePersonalizationType.REV);
        }
    }
}

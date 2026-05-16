using Ums.Shell.Factory.Demo.Interfaces;
using Ums.Shell.Factory.Demo.Models;

namespace Ums.Shell.Factory.Demo.Impl
{
    public class LogicModelCATLoader : ILogicModelLoader
    {
        public ModelLogic Load(ePersonalizationType personalizationType)
        {
            return new ModelLogic()
            {
                Message = "CAT",
            };
        }
    }
}

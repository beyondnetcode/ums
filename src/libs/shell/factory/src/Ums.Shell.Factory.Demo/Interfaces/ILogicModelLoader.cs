using Ums.Shell.Factory.Demo.Models;

namespace Ums.Shell.Factory.Demo.Interfaces
{
    public interface ILogicModelLoader
    {
        ModelLogic Load(ePersonalizationType personalizationType);
    }
}

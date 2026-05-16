using Ums.Shell.Factory.Demo.Models;

namespace Ums.Shell.Factory.Demo.Interfaces
{
    public interface IStrategyBuilder
    {
        Strategy Build(Criteria criteria);
    }
}

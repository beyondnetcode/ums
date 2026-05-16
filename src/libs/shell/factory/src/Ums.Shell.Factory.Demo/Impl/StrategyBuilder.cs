using Ums.Shell.Factory.Interfaces;
using Ums.Shell.Factory.Demo.Interfaces;
using Ums.Shell.Factory.Demo.Models;

namespace Ums.Shell.Factory.Demo.Impl
{
    public class StrategyBuilder : IStrategyBuilder
    {
        private readonly IFactory _factory;

        public StrategyBuilder(IFactory factory) {
            _factory = factory;
        }

        public Strategy Build(Criteria criteria)        
        {

            var logicModelLoader = this._factory.Create<Criteria, ILogicModelLoader>(criteria)[0];

            var strategy = new Strategy
            {
                Name = string.Format("Strategy {0}", criteria.PersonalizationType),
                PersonalizationType = criteria.PersonalizationType,
                ModelLogic = logicModelLoader.Load(criteria.PersonalizationType)
            };            
            
            return strategy;
        }
    }
}

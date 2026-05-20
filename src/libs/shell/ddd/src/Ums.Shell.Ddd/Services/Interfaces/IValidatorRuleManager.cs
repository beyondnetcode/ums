using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.Rules.Interfaces;

namespace Ums.Shell.Ddd.Services.Interfaces
{
    public interface IValidatorRuleManager<TValidator> where TValidator : IRuleValidator
    {
        void Add(TValidator rule);
        void Add(IEnumerable<TValidator> rules);
        void Remove(TValidator rule);
        void Clear();
        ReadOnlyCollection<TValidator> GetValidators();
        ReadOnlyCollection<BrokenRule> GetBrokenRules(RuleContext? context = null);
    }
}

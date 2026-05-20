using Ums.Shell.Ddd.Rules;

namespace Ums.Shell.Ddd.Services.Interfaces
{
    public interface IBrokenRulesManager
    {
        void Add(BrokenRule brokenRule);
        void Add(IReadOnlyCollection<BrokenRule> brokenRules);
        void Remove(BrokenRule brokenRule);
        void Clear();
        ReadOnlyCollection<BrokenRule> GetBrokenRules();
        string GetBrokenRulesAsString();
    }
}

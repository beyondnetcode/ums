using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.Rules.Interfaces;

namespace Ums.Shell.Ddd.Services.Impl
{
    /// <summary>
    /// Represents a collection of validator rules for a specific type.
    /// </summary>
    /// <typeparam name="TValidator">The type of object to validate.</typeparam>
    public class ValidatorRuleManager<TValidator> where TValidator : IRuleValidator
    {
        private readonly List<TValidator> _businessRules = [];

        /// <summary>
        /// Adds a rule to the collection of validator rules.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        public void Add(TValidator rule)
        {
            ArgumentNullException.ThrowIfNull(rule, nameof(rule));

            if (!_businessRules.Any(x => x.GetType() == rule.GetType()))
            {
                _businessRules.Add(rule);
            }
        }

        /// <summary>
        /// Adds a collection of rules to the collection of validator rules.
        /// </summary>
        /// <param name="rules">The rules to add.</param>
        public void Add(IEnumerable<TValidator> rules)
        {
            ArgumentNullException.ThrowIfNull(rules, nameof(rules));

            foreach (var rule in rules)
            {
                Add(rule);
            }
        }

        /// <summary>
        /// Removes a rule from the collection of validator rules.
        /// </summary>
        /// <param name="rule">The rule to remove.</param>
        public void Remove(TValidator rule)
        {
            ArgumentNullException.ThrowIfNull(rule, nameof(rule));

            if (_businessRules.Contains(rule))
            {
                _businessRules.Remove(rule);
            }
        }

        /// <summary>
        /// Clears all the validator rules from the collection.
        /// </summary>
        public void Clear()
        {
            if (_businessRules.Any())
            {
                _businessRules.Clear();
            }
        }

        /// <summary>
        /// Gets a read-only collection of the validator rules.
        /// </summary>
        /// <returns>A read-only collection of the validator rules.</returns>
        public ReadOnlyCollection<TValidator> GetValidators() => _businessRules.ToList().AsReadOnly();

        /// <summary>
        /// Gets a read-only collection of the broken rules based on the specified rule context.
        /// </summary>
        /// <param name="context">The rule context.</param>
        /// <returns>A read-only collection of the broken rules.</returns>
        public ReadOnlyCollection<BrokenRule> GetBrokenRules(RuleContext? context = null)
        {
            var result = new List<BrokenRule>();

            foreach (var rule in _businessRules)
            {
                var brokenRules = rule.Validate(context!);

                if (brokenRules.Any())
                {
                    foreach (var brokenRule in brokenRules)
                    {
                        if (!result.Any(x => x.Property.ToUpperInvariant().Trim() == brokenRule.Property.ToUpperInvariant().Trim()
                                          && x.Message.ToUpperInvariant().Trim() == brokenRule.Message.ToUpperInvariant().Trim()))
                        {
                            result.Add(brokenRule);
                        }
                    }
                }
            }

            return result.AsReadOnly();
        }
    }
}



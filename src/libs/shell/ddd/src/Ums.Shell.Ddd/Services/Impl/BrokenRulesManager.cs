using System.Globalization;
using System.Text;
using Ums.Shell.Ddd.Rules;

namespace Ums.Shell.Ddd.Services.Impl
{
    /// <summary>
    /// Represents a collection of broken rules.
    /// </summary>
    public class BrokenRulesManager
    {
        private List<BrokenRule> _brokenRules = new();

        /// <summary>
        /// Adds a broken rule to the collection.
        /// </summary>
        /// <param name="brokenRule">The broken rule to add.</param>
        public void Add(BrokenRule brokenRule)
        {
            ArgumentNullException.ThrowIfNull(brokenRule, nameof(brokenRule));

            if (!_brokenRules.Exists(x => string.Equals(x.Property, brokenRule.Property, StringComparison.OrdinalIgnoreCase) &&
                                          string.Equals(x.Message, brokenRule.Message, StringComparison.OrdinalIgnoreCase)))
            {
                _brokenRules.Add(brokenRule);
            }
        }


        /// <summary>
        /// Adds a collection of broken rules to the collection.
        /// </summary>
        /// <param name="brokenRules">The collection of broken rules to add.</param>
        public void Add(IReadOnlyCollection<BrokenRule> brokenRules)
        {
            ArgumentNullException.ThrowIfNull(brokenRules, nameof(brokenRules));

            foreach (var brokenRule in brokenRules)
            {
                Add(brokenRule);
            }
        }

        /// <summary>
        /// Removes a broken rule from the collection.
        /// </summary>
        /// <param name="brokenRule">The broken rule to remove.</param>
        public void Remove(BrokenRule brokenRule)
        {
            ArgumentNullException.ThrowIfNull(brokenRule, nameof(brokenRule));

            _brokenRules.Remove(brokenRule);
        }

        /// <summary>
        /// Clears all the broken rules from the collection.
        /// </summary>
        public void Clear()
        {
            _brokenRules.Clear();
        }

        /// <summary>
        /// Gets the read-only collection of broken rules.
        /// </summary>
        /// <returns>The read-only collection of broken rules.</returns>
        public ReadOnlyCollection<BrokenRule> GetBrokenRules()
        {
            return _brokenRules.AsReadOnly();
        }

        /// <summary>
        /// Gets the broken rules of the entity as a string.
        /// </summary>
        /// <returns>The broken rules of the entity as a string.</returns>
        public string GetBrokenRulesAsString()
        {
            if (_brokenRules.Count == 0) return string.Empty;

            var sb = new StringBuilder();

            foreach (var rule in _brokenRules)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"Property: {rule.Property}, Message: {rule.Message}");
            }

            return sb.ToString();
        }
    }
}

using Ums.Shell.Ddd.Rules.Interfaces;

namespace Ums.Shell.Ddd.Rules.Impl
{
    /// <summary>
    /// Represents an abstract base class for rule validators.
    /// </summary>
    /// <typeparam name="T">The type of the subject to be validated.</typeparam>
    public abstract class AbstractRuleValidator<TSubject> : IRuleValidator
    {
        private List<BrokenRule> brokenRules = new List<BrokenRule>();

        public TSubject Subject { get; }

        protected AbstractRuleValidator(TSubject subject)
        {
            Subject = subject;
        }

        /// <summary>
        /// Adds the rules to be validated.
        /// </summary>
        /// <param name="context">The rule context.</param>
        public abstract void AddRules(RuleContext? context);

        /// <summary>
        /// Validates the subject against the rules.
        /// </summary>
        /// <param name="context">The rule context.</param>
        /// <returns>A read-only collection of broken rules.</returns>
        public ReadOnlyCollection<BrokenRule> Validate(RuleContext? context)
        {
            if (Subject != null)
            {
                AddRules(context);
            }

            return brokenRules.AsReadOnly();
        }

        /// <summary>
        /// Adds a broken rule to the collection.
        /// </summary>
        /// <param name="propertyName">The name of the property associated with the broken rule.</param>
        /// <param name="message">The error message of the broken rule.</param>
        public void AddBrokenRule(string propertyName, string message)
        {
            brokenRules.Add(new BrokenRule(propertyName, message));
        }

        public Type GetValidatorDescriptor() => this.GetType();
        public Type GetSubjectDescriptor() => Subject!.GetType();
    }
}

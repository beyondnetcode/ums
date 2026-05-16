namespace Ums.Shell.Ddd.Rules.Interfaces
{
    /// <summary>
    /// Represents a rule validator for a specific type.
    /// </summary>
    public interface IRuleValidator
    {
        /// <summary>
        /// Gets the validator associated with the rule.
        /// </summary>
        Type GetValidatorDescriptor();

        /// <summary>
        /// Gets the name of the subject.
        /// </summary>
        /// <returns></returns>
        Type GetSubjectDescriptor();

        /// <summary>
        /// Validates the rule against the specified context.
        /// </summary>
        /// <param name="context">The rule context.</param>
        /// <returns>A collection of broken rules.</returns>
        ReadOnlyCollection<BrokenRule> Validate(RuleContext context);
    }
}

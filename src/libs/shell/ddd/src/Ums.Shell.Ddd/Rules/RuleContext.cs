namespace Ums.Shell.Ddd.Rules
{
    /// <summary>
    /// Represents the context for a rule.
    /// </summary>
    public class RuleContext
    {
        /// <summary>
        /// Gets the parameters associated with the rule context.
        /// </summary>
        public Collection<(string, object)> Parameters { get; } = new Collection<(string, object)>();
    }
}

namespace Ums.Shell.Ddd.Rules
{
    /// <summary>
    /// Represents a broken rule.
    /// </summary>
    public class BrokenRule
    {
        /// <summary>
        /// Gets the property associated with the broken rule.
        /// </summary>
        public string Property { get; private set; }

        /// <summary>
        /// Gets the message describing the broken rule.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenRule"/> class.
        /// </summary>
        /// <param name="property">The property associated with the broken rule.</param>
        /// <param name="message">The message describing the broken rule.</param>
        public BrokenRule(string property, string message)
        {
            Property = property;
            Message = message;
        }
    }
}

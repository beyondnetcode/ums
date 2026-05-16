using Ums.Shell.Ddd.ValueObjects.Validators;

namespace Ums.Shell.Ddd.ValueObjects.Common
{
    /// <summary>
    /// Represents a string value object.
    /// </summary>
    public class StringValueObject : ValueObject<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringValueObject"/> class.
        /// </summary>
        /// <param name="value">The string value.</param>
        protected StringValueObject(string value) : base(value)
        {
        }      

        /// <summary>
        /// Gets the equality components of the string value object.
        /// </summary>
        /// <returns>An enumerable of the equality components.</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GetValue();
        }

        public override void AddValidators()
        {
            base.AddValidators();

            ValidatorRules.Add(new StringNotNullValidator(this));
        }

        /// <summary>
        /// Gets the default value of the string value object.
        /// </summary>
        public static StringValueObject DefaultValue => new StringValueObject(string.Empty);
    }
}

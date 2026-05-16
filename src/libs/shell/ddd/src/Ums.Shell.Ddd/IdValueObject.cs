using Ums.Shell.Ddd.Helpers;

namespace Ums.Shell.Ddd
{
    /// <summary>
    /// Represents an identifier value object.
    /// </summary>
    public class IdValueObject : ValueObject<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdValueObject"/> class with the specified value.
        /// </summary>
        /// <param name="value">The GUID value of the identifier.</param>
        protected IdValueObject(Guid value) : base(value)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IdValueObject"/> with a random identifier value.
        /// </summary>
        /// <returns>A new <see cref="IdValueObject"/> instance with a randomly generated GUID.</returns>
        public static IdValueObject Create()
        {
            return new IdValueObject(Guid.NewGuid());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IdValueObject"/> with the specified identifier value as a string.
        /// </summary>
        /// <param name="value">The string representation of the identifier value.</param>
        /// <returns>A new <see cref="IdValueObject"/> instance with the specified GUID value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null.</exception>
        public static IdValueObject Load(string value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            return new IdValueObject(IdHelper.GetGuidFromString(value));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IdValueObject"/> with the specified identifier value as a GUID.
        /// </summary>
        /// <param name="value">The GUID value of the identifier.</param>
        /// <returns>A new <see cref="IdValueObject"/> instance with the specified GUID value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> is null.</exception>
        public static IdValueObject Load(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            return new IdValueObject(value);
        }

        /// <summary>
        /// Gets the components that are used to determine equality for the value object.
        /// </summary>
        /// <returns>An enumerable of objects representing the equality components.</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return GetValue();
        }

        /// <summary>
        /// Gets the default value of the <see cref="IdValueObject"/>, which is an empty GUID.
        /// </summary>
        public static IdValueObject DefaultValue => new IdValueObject(Guid.Empty);
    }
}
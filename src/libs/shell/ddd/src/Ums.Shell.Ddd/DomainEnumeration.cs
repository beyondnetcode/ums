using System.ComponentModel.DataAnnotations;

namespace Ums.Shell.Ddd
{
    /// <summary>
    /// Represents an abstract base class for enumerations.
    /// </summary>
    public abstract class DomainEnumeration : IComparable
    {
        /// <summary>
        /// Gets the name of the enumeration.
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the ID of the enumeration.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEnumeration"/> class.
        /// </summary>
        /// <param name="id">The ID of the enumeration.</param>
        /// <param name="name">The name of the enumeration.</param>
        protected DomainEnumeration(int id, string name) => (Id, Name) = (id, name);

        /// <summary>
        /// Returns the name of the enumeration.
        /// </summary>
        /// <returns>The name of the enumeration.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Gets all the values of the specified enumeration type.
        /// </summary>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <returns>An enumerable collection of all the values of the specified enumeration type.</returns>
        public static IEnumerable<T> GetAll<T>() where T : DomainEnumeration =>
            typeof(T).GetFields(BindingFlags.Public |
                                BindingFlags.Static |
                                BindingFlags.DeclaredOnly)
                        .Select(f => f.GetValue(null))
                        .Cast<T>();

        /// <summary>
        /// Determines whether the current enumeration object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current enumeration object.</param>
        /// <returns><c>true</c> if the current enumeration object is equal to the other object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not DomainEnumeration otherValue)
            {
                return false;
            }

            var typeMatches = GetType().Equals(obj?.GetType());

            var valueMatches = Id.Equals(otherValue.Id);

            return typeMatches && valueMatches;
        }

        /// <summary>
        /// Returns the hash code for the current enumeration object.
        /// </summary>
        /// <returns>The hash code for the current enumeration object.</returns>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Calculates the absolute difference between two enumeration values.
        /// </summary>
        /// <param name="firstValue">The first enumeration value.</param>
        /// <param name="secondValue">The second enumeration value.</param>
        /// <returns>The absolute difference between the two enumeration values.</returns>
        public static int AbsoluteDifference(DomainEnumeration firstValue, DomainEnumeration secondValue)
        {
            if (firstValue is null)
            {
                throw new ArgumentNullException(nameof(firstValue));
            }

            if (secondValue is null)
            {
                throw new ArgumentNullException(nameof(secondValue));
            }

            var absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);

            return absoluteDifference;
        }

        /// <summary>
        /// Retrieves the enumeration value from the specified integer value.
        /// </summary>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <param name="value">The integer value.</param>
        /// <returns>The enumeration value that matches the specified integer value, or <c>null</c> if no match is found.</returns>
        public static T? FromValue<T>(int value) where T : DomainEnumeration
        {
            var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
            return matchingItem;
        }

        /// <summary>
        /// Retrieves the enumeration value from the specified display name.
        /// </summary>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <param name="displayName">The display name.</param>
        /// <returns>The enumeration value that matches the specified display name, or <c>null</c> if no match is found.</returns>
        public static T? FromDisplayName<T>(string displayName) where T : DomainEnumeration
        {
            var matchingItem = Parse<T, string>(displayName, "display name", item => item.Name == displayName);

            return matchingItem;
        }

        private static T? Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : DomainEnumeration
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);

            return matchingItem;
        }

        /// <summary>
        /// Compares the current enumeration object with another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current enumeration object.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CA1062 // Validate arguments of public methods
        public int CompareTo(object obj) => Id.CompareTo(((DomainEnumeration)obj).Id);
#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

        /// <summary>
        /// Sets the value of the enumeration based on the specified integer value.
        /// </summary>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <param name="value">The integer value.</param>
        /// <returns>The enumeration value that matches the specified integer value.</returns>
        public static T? SetValue<T>(int value) where T : DomainEnumeration
        {
            var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);

            return matchingItem;
        }

        /// <summary>
        /// Determines whether two enumeration objects are equal.
        /// </summary>
        /// <param name="left">The first enumeration object.</param>
        /// <param name="right">The second enumeration object.</param>
        /// <returns><c>true</c> if the two enumeration objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DomainEnumeration left, DomainEnumeration right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two enumeration objects are not equal.
        /// </summary>
        /// <param name="left">The first enumeration object.</param>
        /// <param name="right">The second enumeration object.</param>
        /// <returns><c>true</c> if the two enumeration objects are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DomainEnumeration left, DomainEnumeration right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the first enumeration object is less than the second enumeration object.
        /// </summary>
        /// <param name="left">The first enumeration object.</param>
        /// <param name="right">The second enumeration object.</param>
        /// <returns><c>true</c> if the first enumeration object is less than the second enumeration object; otherwise, <c>false</c>.</returns>
        public static bool operator <(DomainEnumeration left, DomainEnumeration right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Determines whether the first enumeration object is less than or equal to the second enumeration object.
        /// </summary>
        /// <param name="left">The first enumeration object.</param>
        /// <param name="right">The second enumeration object.</param>
        /// <returns><c>true</c> if the first enumeration object is less than or equal to the second enumeration object; otherwise, <c>false</c>.</returns>
        public static bool operator <=(DomainEnumeration left, DomainEnumeration right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Determines whether the first enumeration object is greater than the second enumeration object.
        /// </summary>
        /// <param name="left">The first enumeration object.</param>
        /// <param name="right">The second enumeration object.</param>
        /// <returns><c>true</c> if the first enumeration object is greater than the second enumeration object; otherwise, <c>false</c>.</returns>
        public static bool operator >(DomainEnumeration left, DomainEnumeration right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Determines whether the first enumeration object is greater than or equal to the second enumeration object.
        /// </summary>
        /// <param name="left">The first enumeration object.</param>
        /// <param name="right">The second enumeration object.</param>
        /// <returns><c>true</c> if the first enumeration object is greater than or equal to the second enumeration object; otherwise, <c>false</c>.</returns>
        public static bool operator >=(DomainEnumeration left, DomainEnumeration right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}



namespace Ums.Shell.Ddd.Rules.PropertyChange
{
    internal static class ReflectionTypeExtensions
    {
        /// <summary>
        /// Determines whether the specified type is a value type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the specified type is a value type; otherwise, <c>false</c>.</returns>
        internal static bool GetIsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        /// <summary>
        /// Determines whether the specified type is assignable from another type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="secondType">The type to compare with the current type.</param>
        /// <returns><c>true</c> if the specified type is assignable from the other type; otherwise, <c>false</c>.</returns>
        internal static bool GetIsAssignableFrom(this Type type, Type secondType)
        {
            return type.GetTypeInfo().IsAssignableFrom(secondType.GetTypeInfo());
        }
    }
}

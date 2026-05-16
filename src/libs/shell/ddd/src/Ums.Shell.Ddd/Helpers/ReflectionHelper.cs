namespace Ums.Shell.Ddd.Helpers
{
    /// <summary>
    /// A helper class for reflection-related operations.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Determines whether the specified type is a subclass of the specified generic type.
        /// </summary>
        /// <param name="generic">The generic type to check against.</param>
        /// <param name="toCheck">The type to check.</param>
        /// <returns><c>true</c> if the type is a subclass of the generic type; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="generic"/> or <paramref name="toCheck"/> is null.</exception>
        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            if (generic is null)
            {
                throw new ArgumentNullException(nameof(generic));
            }

            if (toCheck is null)
            {
                throw new ArgumentNullException(nameof(toCheck));
            }

            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType!;
            }
            return false;
        }
    }
}

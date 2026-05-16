namespace Ums.Shell.Ddd.Extensions
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> PrettyPrintCache = new();

        /// <summary>
        /// Returns a pretty-printed string representation of the specified type.
        /// </summary>
        /// <param name="type">The type to be pretty-printed.</param>
        /// <returns>A pretty-printed string representation of the type.</returns>
        public static string Print(this Type type)
        {
            return PrettyPrintCache.GetOrAdd(
                type,
                t =>
                {
                    try
                    {
                        return PrettyPrintRecursive(t, 0);
                    }
                    catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                    {
                        return t.Name;
                    }
                });
        }

        private static readonly ConcurrentDictionary<Type, string> TypeCacheKeys = new();

        /// <summary>
        /// Returns a cache key for the specified type.
        /// </summary>
        /// <param name="type">The type for which to get the cache key.</param>
        /// <returns>A cache key for the type.</returns>
        public static string GetCacheKey(this Type type)
        {
            return TypeCacheKeys.GetOrAdd(
                type,
                t => $"{t.Print()}[hash: {t.GetHashCode()}]");
        }

        private static string PrettyPrintRecursive(Type type, int depth)
        {
            if (depth > 3)
            {
                return type.Name;
            }

            var nameParts = type.Name.Split('`');
            if (nameParts.Length == 1)
            {
                return nameParts[0];
            }

            var genericArguments = type.GetGenericArguments();
            return !type.IsConstructedGenericType
                ? $"{nameParts[0]}<{new string(',', genericArguments.Length - 1)}>"
                : $"{nameParts[0]}<{string.Join(",", genericArguments.Select(t => PrettyPrintRecursive(t, depth + 1)))}>";
        }

        /// <summary>
        /// Determines whether the type has a constructor with a parameter of the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="predicate">The predicate to match the parameter type.</param>
        /// <returns><c>true</c> if the type has a constructor with a parameter of the specified type; otherwise, <c>false</c>.</returns>
        internal static bool HasConstructorParameterOfType(this Type type, Predicate<Type> predicate)
        {
            return type.GetConstructors()
                .Any(c => c.GetParameters()
                    .Any(p => predicate(p.ParameterType)));
        }

        /// <summary>
        /// Determines whether the type is assignable to the specified generic type.
        /// </summary>
        /// <typeparam name="T">The generic type to check against.</typeparam>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is assignable to the specified generic type; otherwise, <c>false</c>.</returns>
        internal static bool IsAssignableTo<T>(this Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }
    }
}

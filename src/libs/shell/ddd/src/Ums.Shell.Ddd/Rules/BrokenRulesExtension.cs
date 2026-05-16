using Ums.Shell.Ddd.Helpers;
using Ums.Shell.Ddd.Services.Impl;

namespace Ums.Shell.Ddd.Rules
{
    public static class BrokenRulesExtension
    {
        private const string BrokenRulesPropertyName = "BrokenRules";

        /// <summary>
        /// Gets the broken rules for the properties of the specified entity instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="properties">The properties to check for broken rules.</param>
        /// <param name="instance">The instance of the entity.</param>
        /// <returns>A read-only collection of broken rules.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="properties"/> is null.</exception>
        public static ReadOnlyCollection<BrokenRule> GetPropertiesBrokenRules<TEntity>(this PropertyInfo[] properties,
                                                                               TEntity instance)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (properties is null)
                throw new ArgumentNullException(nameof(properties));

            var result = new List<BrokenRule>();

            foreach (var property in properties)
            {
                if (!ReflectionHelper.IsSubclassOfRawGeneric(typeof(ValueObject<>), property.PropertyType))
                {
                    continue;
                }

                var valueObject = property.GetValue(instance);
                
                if (valueObject is null)
                {
                    continue;
                }

                var brokenRulesProperty = valueObject.GetType().GetProperty(BrokenRulesPropertyName);
                
                if (brokenRulesProperty is null)
                {
                    continue;
                }

                var brokenRuleProperty = (BrokenRulesManager)brokenRulesProperty.GetValue(valueObject)!;
                var brokenRules = brokenRuleProperty.GetBrokenRules();
                
                if (brokenRules.Count > 0)
                {
                    result.AddRange(brokenRules);
                }
            }

            return result.AsReadOnly();
        }
    }
}

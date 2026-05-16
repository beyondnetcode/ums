using Ums.Shell.Ddd.Rules.Impl;
using Ums.Shell.Ddd.Services.Impl;

namespace Ums.Shell.Ddd.Interfaces
{
    /// <summary>
    /// Represents an entity with specific properties and validation rules.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProps">The type of the properties.</typeparam>
    public interface IEntity<TEntity, TProps>
        where TEntity : class
        where TProps : class, IProps
    {
        /// <summary>
        /// Gets the manager for broken rules.
        /// </summary>
        BrokenRulesManager BrokenRules { get; }

        /// <summary>
        /// Gets the identifier value object.
        /// </summary>
        IdValueObject Id { get; }

        /// <summary>
        /// Gets the properties of the entity.
        /// </summary>
        TProps Props { get; }

        /// <summary>
        /// Gets the tracking state manager.
        /// </summary>
        TrackingStateManager TrackingState { get; }

        /// <summary>
        /// Gets the validator rule manager.
        /// </summary>
        ValidatorRuleManager<AbstractRuleValidator<TEntity>> ValidatorRules { get; }

        /// <summary>
        /// Adds validators to the entity.
        /// </summary>
        void AddValidators();

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        bool Equals(object? obj);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        int GetHashCode();

        /// <summary>
        /// Gets a copy of the properties.
        /// </summary>
        /// <returns>A copy of the properties.</returns>
        TProps GetPropsCopy();

        /// <summary>
        /// Determines whether the entity is valid.
        /// </summary>
        /// <returns>true if the entity is valid; otherwise, false.</returns>
        bool IsValid();

        /// <summary>
        /// Sets the identifier value object.
        /// </summary>
        /// <param name="id">The identifier string.</param>
        /// <returns>The identifier value object.</returns>
        IdValueObject SetId(string id);

        /// <summary>
        /// Sets the properties of the entity.
        /// </summary>
        /// <param name="props">The properties to set.</param>
        void SetProps(TProps props);

        /// <summary>
        /// Validates the entity.
        /// </summary>
        void Validate();
    }
}
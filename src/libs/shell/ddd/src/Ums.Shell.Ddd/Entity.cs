using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.Extensions;
using Ums.Shell.Ddd.Rules.Impl;
using Ums.Shell.Ddd.Services.Impl;

namespace Ums.Shell.Ddd
{
    /// <summary>
    /// Represents an abstract base class for entities in the domain-driven design.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProps">The type of the entity properties.</typeparam>
    public abstract class Entity<TEntity, TProps> : IEntity<TEntity, TProps> where TEntity : class
            where TProps : class, IProps
    {
        #region Members         

        /// <summary>
        /// The properties of the entity.
        /// </summary>
        private TProps _props;

        #endregion

        #region Properties 

        public IdValueObject Id { get; private set; }

        public IdValueObject SetId(string id)
        {
            return IdValueObject.Load(id);
        }

        public BrokenRulesManager BrokenRules { get;  }

        public TrackingStateManager TrackingState { get; }

        public ValidatorRuleManager<AbstractRuleValidator<TEntity>> ValidatorRules { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{TEntity, TProps}"/> class.
        /// </summary>
        /// <param name="props">The properties of the entity.</param>
        protected Entity(TProps props)
        {
            BrokenRules = new BrokenRulesManager();

            TrackingState = new TrackingStateManager();

            ValidatorRules = new ValidatorRuleManager<AbstractRuleValidator<TEntity>>();

            Id = IdValueObject.Create();

            _props = props;

            Validate();

            TrackingState.MarkAsNew();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a copy of the entity properties.
        /// </summary>
        /// <returns>A copy of the entity properties.</returns>
        public TProps GetPropsCopy()
        {
            var copyProps = _props!.Clone();

            return (TProps)copyProps;
        }

        /// <summary>
        /// Gets the properties of the entity.
        /// </summary>
        public TProps Props
        {
            get { return _props!; }
        }

        /// <summary>
        /// Sets the properties of the entity.
        /// </summary>
        /// <param name="props">The properties to set.</param>
        public void SetProps(TProps props)
        {
            _props = props;

            TrackingState.MarkAsDirty();
        }

        #endregion

        #region BusinessRules

        /// <summary>
        /// Gets a value indicating whether the entity is valid.
        /// </summary>
        /// <returns><c>true</c> if the entity is valid; otherwise, <c>false</c>.</returns>
        public bool IsValid()
        {
            Validate();

            return !BrokenRules.GetBrokenRules().Any();
        }

        /// <summary>
        /// Validates the entity.
        /// </summary>
        public void Validate()
        {
            Guard();

            // Add validators for Entity
            AddValidators();

            // Get broken rules for Entity
            BrokenRules.Add(ValidatorRules.GetBrokenRules().ToList());

            if (BrokenRules.GetBrokenRules().Any())
            {
                TrackingState.MarkAsDirty();
                return;
            }

            // Explore broken rules for properties
            var props = GetPropsCopy().GetType().GetProperties();

            var propsBrokenRules = props.GetPropertiesBrokenRules(_props);

            if (propsBrokenRules.Any())
            {
                BrokenRules.Add(propsBrokenRules);

                TrackingState.MarkAsDirty();
            }
        }

        private void Guard()
        {              
            if (!(this is TEntity))
                throw new InvalidOperationException($"Entity '{GetType().Print()}' specifies '{typeof(TEntity).Print()}' as generic argument, it should be its own type");
        }

        /// <summary>
        /// Adds the validators for the value object.
        /// </summary>
        public virtual void AddValidators()
        {

        }

        #endregion

        #region Equality

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Entity<TEntity, TProps>))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (GetType() != obj.GetType())
                return false;

            return ReferenceEntityPropertiesEquals(obj);
        }

        private bool ReferenceEntityPropertiesEquals(object? obj)
        {
            if (obj is not Entity<TEntity, TProps> entity)
                return false;

            return Id.Equals(entity.Id);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Determines whether two entities are equal.
        /// </summary>
        /// <param name="left">The left entity.</param>
        /// <param name="right">The right entity.</param>
        /// <returns><c>true</c> if the entities are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Entity<TEntity, TProps> left, Entity<TEntity, TProps> right)
        {
            if (Equals(left, null))
                return Equals(right, null) ? true : false;
            else
                return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two entities are not equal.
        /// </summary>
        /// <param name="left">The left entity.</param>
        /// <param name="right">The right entity.</param>
        /// <returns><c>true</c> if the entities are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Entity<TEntity, TProps> left, Entity<TEntity, TProps> right)
        {
            return !(left == right);
        }

        #endregion

        #region TransitionStatus

        /// <summary>
        /// Finite State Machine (FSM) Pattern
        /// </summary>

        private readonly Dictionary<object, List<object>> _validTransitions = new();

        protected void DefineValidTransitions(Dictionary<object, List<object>> transitions)
        {
            ArgumentNullException.ThrowIfNull(transitions, nameof(transitions));

            _validTransitions.Clear();
            foreach (var transition in transitions)
            {
                _validTransitions[transition.Key] = transition.Value;
            }
        }

        protected bool CanTransitionTo(object currentState, object newState)
        {
            if (!_validTransitions.ContainsKey(currentState))
            {
                throw new InvalidOperationException($"No transitions defined for state {currentState}.");
            }

            return _validTransitions[currentState].Contains(newState);
        }

        #endregion
    }
}


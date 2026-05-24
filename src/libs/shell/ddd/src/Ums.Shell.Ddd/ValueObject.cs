using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.Rules.Impl;
using Ums.Shell.Ddd.Rules.PropertyChange;
using Ums.Shell.Ddd.Services.Impl;
using Ums.Shell.Ddd.Services.Interfaces;

namespace Ums.Shell.Ddd
{
    /// <summary>
    /// Base class for value objects in the domain-driven design.
    /// </summary>
    /// <typeparam name="TValue">The type of the value object.</typeparam>
    public abstract class ValueObject<TValue> : AbstractNotifyPropertyChanged, IProps
    {
        #region Members

        public ITrackingStateManager TrackingState { get; private set; }

        public ValidatorRuleManager<AbstractRuleValidator<ValueObject<TValue>>> ValidatorRules { get; private set; }

        public IBrokenRulesManager BrokenRules { get; private set; }

        public bool IsValid => !BrokenRules.GetBrokenRules().Any();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value of the value object.
        /// </summary>
        protected TValue InternalValue
        {
            get
            {
                return (TValue)GetValue();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueObject{TValue}"/> class.
        /// </summary>
        /// <param name="value">The value of the value object.</param>
        protected ValueObject(TValue value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            BrokenRules = new BrokenRulesManager();

            ValidatorRules = new ValidatorRuleManager<AbstractRuleValidator<ValueObject<TValue>>>();

            TrackingState = new TrackingStateManager();

            RegisterProperty(nameof(InternalValue), typeof(TValue), value, ValuePropertyChanged);

            TrackingState.MarkAsNew();

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            AddValidators();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
            Validate();
        }

        #endregion

        #region Methods

        private void ValuePropertyChanged(AbstractNotifyPropertyChanged sender, NotifyPropertyChangedContextArgs e)
        {
            TrackingState.MarkAsDirty();

            Validate();
        }

        public void SetValue(TValue value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            SetValue(value, nameof(InternalValue));
        }

        public TValue GetValue()
        {
            return (TValue)GetValue(nameof(InternalValue));
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Adds the validators for the value object.
        /// </summary>
        public virtual void AddValidators()
        {

        }

        private void Validate()
        {
            //BrokenRules.Clear();

            BrokenRules.Add(ValidatorRules.GetBrokenRules());
        }

        #endregion

        #region Equality

        protected static bool EqualOperator(ValueObject<TValue> left, ValueObject<TValue> right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right!);
        }

        protected static bool NotEqualOperator(ValueObject<TValue> left, ValueObject<TValue> right)
        {
            return !(EqualOperator(left, right));
        }

        protected abstract IEnumerable<object> GetEqualityComponents();

        /// <summary>
        /// Determines whether the current value object is equal to another value object.
        /// </summary>
        /// <param name="obj">The value object to compare with the current value object.</param>
        /// <returns><c>true</c> if the current value object is equal to the other value object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject<TValue>)obj;

            return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// Gets the hash code of the value object.
        /// </summary>
        /// <returns>The hash code of the value object.</returns>
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// Creates a copy of the value object.
        /// </summary>
        /// <returns>A copy of the value object.</returns>
        public ValueObject<TValue> GetCopy()
        {
            return (ValueObject<TValue>)MemberwiseClone();
        }

        public object Clone()
        {
            return GetCopy();
        }


        #endregion
    }
}

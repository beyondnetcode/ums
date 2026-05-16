namespace Ums.Shell.Ddd.Rules.PropertyChange
{
    /// <summary>
    /// Delegate for handling property changed events.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event arguments containing the property change information.</param>
    public delegate void NotifyPropertyChangedHandler(AbstractNotifyPropertyChanged sender, NotifyPropertyChangedContextArgs e);

    /// <summary>
    /// Base class for implementing the INotifyPropertyChanged interface.
    /// </summary>
    public class AbstractNotifyPropertyChanged : INotifyPropertyChanged
    {
        #region Members

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Dictionary<string, NotifyPropertyChangedContext> properties = new Dictionary<string, NotifyPropertyChangedContext>();

        /// <summary>
        /// Gets or sets a value indicating whether invoking property changed callbacks is enabled.
        /// </summary>
        protected bool IsCallbackInvokingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether invoking property changed events is enabled.
        /// </summary>
        protected bool IsEventInvokingEnabled { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractNotifyPropertyChanged"/> class.
        /// </summary>
        protected AbstractNotifyPropertyChanged()
        {
            IsCallbackInvokingEnabled = true;
            IsEventInvokingEnabled = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a property with the specified name, type, and default value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="type">The type of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        protected void RegisterProperty(string name, Type type, object defaultValue)
        {
            RegisterProperty(name, type, defaultValue, null);
        }

        /// <summary>
        /// Registers a property with the specified name, type, default value, and property changed handler.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="type">The type of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="notifyPropertyChangedHandler">The handler to be called when the property changes.</param>
        protected void RegisterProperty(string name, Type type, object defaultValue, NotifyPropertyChangedHandler? notifyPropertyChangedHandler)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            ValidateValueForType(defaultValue, type);

            if (properties.ContainsKey(name))
            {
                throw new ArgumentException($"This class already contains registered property named '{name}'.");
            }

            properties.Add(name, new NotifyPropertyChangedContext(defaultValue, type, notifyPropertyChangedHandler));
        }

        /// <summary>
        /// Registers a callback to be invoked when the specified property changes.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="notifyPropertyChangedHandler">The handler to be called when the property changes.</param>
        public void RegisterPropertyChangedCallback(string propertyName, NotifyPropertyChangedHandler notifyPropertyChangedHandler)
        {
            ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
            ArgumentNullException.ThrowIfNull(notifyPropertyChangedHandler, nameof(notifyPropertyChangedHandler));

            GetPropertyContext(propertyName, nameof(propertyName)).PropertyChangedCallback += notifyPropertyChangedHandler;
        }

        /// <summary>
        /// Unregisters a callback that was previously registered for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="notifyPropertyChangedHandler">The handler to be unregistered.</param>
        protected void UnregisterPropertyChangedCallback(string propertyName, NotifyPropertyChangedHandler notifyPropertyChangedHandler)
        {
            ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

            GetPropertyContext(propertyName, nameof(propertyName)).PropertyChangedCallback -= notifyPropertyChangedHandler;
        }

        /// <summary>
        /// Gets the value of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        protected object GetValue([CallerMemberName] string propertyName = default!)
        {
            return GetPropertyContext(propertyName, nameof(propertyName)).Value;
        }

        /// <summary>
        /// Sets the value of the specified property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        public void ForceSetValue(object value, [CallerMemberName] string propertyName = default!)
        {
            SetValue(value, propertyName, true);
        }

        /// <summary>
        /// Sets the value of the specified property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        protected void SetValue(object value, [CallerMemberName] string propertyName = default!)
        {
            SetValue(value, propertyName, false);
        }

        private void SetValue(object value, string propertyName, bool forceSetValue)
        {
            NotifyPropertyChangedContext propertyData = GetPropertyContext(propertyName, nameof(propertyName));

            ValidateValueForType(value, propertyData.Type);

            // Calling Equals calls the overridden method even when the value is boxed
            bool? valuesEqual = propertyData.Value?.Equals(value);

            if (forceSetValue || valuesEqual == null && !ReferenceEquals(value, null) || valuesEqual == false)
            {
                object oldValue = propertyData.Value!;

                propertyData.Value = value!;

                if (IsCallbackInvokingEnabled)
                {
                    propertyData.InvokePropertyChangedCallback(this, new NotifyPropertyChangedContextArgs(oldValue, value!));
                }

                if (IsEventInvokingEnabled)
                {
                    OnNotifyPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected virtual void OnNotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets the property context for the specified property name.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyNameParameterName">The name of the parameter containing the property name.</param>
        /// <returns>The property context for the specified property name.</returns>
        private NotifyPropertyChangedContext GetPropertyContext(string propertyName, string propertyNameParameterName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(propertyName, propertyNameParameterName);

            try
            {
                return properties[propertyName];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"There is no registered property called '{propertyName}'.", propertyNameParameterName);
            }
        }

        /// <summary>
        /// Validates the value for the specified type.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="type">The type to validate against.</param>
        private static void ValidateValueForType(object value, Type type)
        {
            if (value == null)
            {
                if (type.GetIsValueType() && Nullable.GetUnderlyingType(type) == null)
                {
                    throw new ArgumentException($"The type '{type}' is not a nullable type.");
                }
            }
            else
            {
                if (!type.GetIsAssignableFrom(value.GetType()))
                {
                    throw new ArgumentException($"The specified value cannot be assigned to a property of type ({type})");
                }
            }
        }

        #endregion
    }
}

namespace Ums.Shell.Ddd.Rules.PropertyChange
{
    /// <summary>
    /// Represents the context for notifying property changes.
    /// </summary>
    public class NotifyPropertyChangedContext
    {
        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        internal object Value { get; set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        internal Type Type { get; }

        /// <summary>
        /// Occurs when the property is changed.
        /// </summary>
        internal event NotifyPropertyChangedHandler? PropertyChangedCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedContext"/> class.
        /// </summary>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="type">The type of the property.</param>
        /// <param name="notifyPropertyChangedHandler">The handler for property changed event.</param>
        internal NotifyPropertyChangedContext(object defaultValue, Type type, NotifyPropertyChangedHandler? notifyPropertyChangedHandler)
        {
            Value = defaultValue;
            Type = type;

            PropertyChangedCallback += notifyPropertyChangedHandler;
        }

        /// <summary>
        /// Invokes the property changed callback.
        /// </summary>
        /// <param name="sender">The sender of the property changed event.</param>
        /// <param name="e">The event arguments for the property changed event.</param>
        internal void InvokePropertyChangedCallback(AbstractNotifyPropertyChanged sender, NotifyPropertyChangedContextArgs e)
        {
            PropertyChangedCallback?.Invoke(sender, e);
        }
    }
}

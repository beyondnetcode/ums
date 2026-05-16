namespace Ums.Shell.Ddd.Rules.PropertyChange
{
    public sealed class NotifyPropertyChangedContextArgs
    {
        /// <summary>
        /// Gets or sets a value that marks the callback as handled.
        /// </summary>
        public bool Handled { get; set; }
        /// <summary>
        /// Gets the previous value of the changed property.
        /// </summary>
        public object PreviusValue { get; }
        /// <summary>
        /// Gets the current value of the changed property.
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedContextArgs"/> class.
        /// </summary>
        /// <param name="previusValue">Previous value of the changed property.</param>
        /// <param name="newValue">Current value of the changed property.</param>
        public NotifyPropertyChangedContextArgs(object previusValue, object newValue)
        {
            Handled = false;
            PreviusValue = previusValue;
            NewValue = newValue;
        }
    }
}

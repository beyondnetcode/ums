using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.Services.Interfaces;

namespace Ums.Shell.Ddd.Services.Impl
{
    /// <summary>
    /// Represents a tracking mechanism for detecting changes in objects.
    /// </summary>
    public class TrackingStateManager : ITrackingStateManager
    {
        private const string TrackingKeyName = "Tracking";

        /// <summary>
        /// Gets a value indicating whether the object is new.
        /// </summary>
        public bool IsNew { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the object is dirty (has changes).
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the object is self-deleted.
        /// </summary>
        public bool IsSelfDeleted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the object is deleted. 
        /// </summary>
        public bool IsDeleted { get; private set; }

        public TrackingStateManager()
        {
            MarkAsClean();
        }

        /// <summary>
        /// Gets the tracking object for the specified properties.
        /// </summary>
        /// <typeparam name="TProp">The type of the properties.</typeparam>
        /// <param name="props">The properties to check for changes.</param>
        /// <returns>The tracking object.</returns>
        public TrackingStateManager GetTracking<TProp>(TProp props) where TProp : IProps
        {
            return FindChanges(props);
        }

        /// <summary>
        /// Marks the object as dirty (has changes).
        /// </summary>
        /// <returns>The tracking object.</returns>
        public void MarkAsDirty()
        {
            IsDirty = true;
            IsNew = false;
            IsSelfDeleted = false;
            IsDeleted = false;
        }


        /// <summary>
        /// Marks the object as new.
        /// </summary>
        /// <returns>The tracking object.</returns>
        public void MarkAsNew()
        {
            IsDirty = false;
            IsNew = true;
            IsSelfDeleted = false;
            IsDeleted = false;
        }

        /// <summary>
        /// Marks the object as deleted.
        /// </summary>
        /// <returns>The tracking object</returns>
        public void MarkAsSelfDeleted()
        {
            IsDirty = false;
            IsNew = false;
            IsSelfDeleted = true;
            IsDeleted = false;
        }

        /// <summary>
        /// Marks the object as deleted.
        /// </summary>
        /// <returns>The tracking object</returns>
        public void MarkAsDeleted()
        {
            IsDirty = false;
            IsNew = false;
            IsSelfDeleted = false;
            IsDeleted = true;
        }

        /// <summary>
        /// Marks the object as clean (no changes).
        /// </summary>
        /// <returns>The tracking object.</returns>
        public void MarkAsClean()
        {
            IsDirty = false;
            IsNew = false;
            IsSelfDeleted = false;
            IsDeleted = false;
        }

        /// <summary>
        /// Finds changes in the specified properties.
        /// </summary>
        /// <typeparam name="TProp">The type of the properties.</typeparam>
        /// <param name="props">The properties to check for changes.</param>
        /// <returns><c>Tracking</c> if changes are found; otherwise, <c>false</c>.</returns>
        protected TrackingStateManager FindChanges<TProp>(TProp props) where TProp : IProps
        {
            MarkAsClean();

            ArgumentNullException.ThrowIfNull(props, nameof(props));

            foreach (var prop in props.GetType().GetProperties())
            {
                var value = prop.GetValue(props);

                if (value == null) continue;

                var trackingProperty = value.GetType().GetProperty(TrackingKeyName);

                if (trackingProperty != null)
                {
                    var trackingValue = (TrackingStateManager)trackingProperty.GetValue(value)!;

                    if (trackingValue.IsDirty) MarkAsDirty();
                    if (trackingValue.IsNew) MarkAsNew();
                    if (trackingValue.IsSelfDeleted) MarkAsSelfDeleted();
                    if (trackingValue.IsDeleted) MarkAsDeleted();
                }
            }

            return this;
        }
    }
}

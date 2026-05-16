using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.Services.Impl;

namespace Ums.Shell.Ddd.Services.Interfaces
{
    public interface ITrackingStateManager
    {
        bool IsDeleted { get; }
        bool IsDirty { get; }
        bool IsNew { get; }
        bool IsSelftDeleted { get; }

        TrackingStateManager GetTracking<TProp>(TProp props) where TProp : IProps;
        void MarkAsClean();
        void MarkAsDeleted();
        void MarkAsDirty();
        void MarkAsNew();
        void MarkAsSelfDeleted();
    }
}
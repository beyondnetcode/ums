using Ums.Shell.Ddd.Services.Impl;
using Ums.Shell.Ddd.Services.Interfaces;

namespace Ums.Shell.Ddd.Factories
{
    public static class DddServiceFactory
    {
        private static Func<IBrokenRulesManager> _brokenRulesFactory = () => new BrokenRulesManager();
        private static Func<ITrackingStateManager> _trackingStateFactory = () => new TrackingStateManager();

        public static void Configure(
            Func<IBrokenRulesManager>? brokenRulesFactory = null,
            Func<ITrackingStateManager>? trackingStateFactory = null)
        {
            _brokenRulesFactory = brokenRulesFactory ?? _brokenRulesFactory;
            _trackingStateFactory = trackingStateFactory ?? _trackingStateFactory;
        }

        public static IBrokenRulesManager CreateBrokenRulesManager() => _brokenRulesFactory();

        public static ITrackingStateManager CreateTrackingStateManager() => _trackingStateFactory();
    }
}

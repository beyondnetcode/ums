using System;
using Serilog;

namespace Ums.Shell.Aop.Aspects.Logger.Serilog
{
    public class SerilogLogger : ILogger
    {
        public readonly string OnExceptionTemplate = "[{ClassName}, {MethodName}] Exception.";

        public readonly string OnExceptionTemplateWithRequestId = "[{ClassName}, {MethodName}, {Id}] Exception.";

        public readonly string OnEntryTemplate = "[{ClassName}, {MethodName}] Start Call.";

        public readonly string OnEntryTemplateWithRequestId = "[{ClassName}, {MethodName}, {Id}] Start Call.";

        public readonly string OnExitTemplateWithDuration = "[{ClassName}, {MethodName}] End Call. Took {Duration} ms.";

        public readonly string OnExitTemplate = "[{ClassName}, {MethodName}] End Call.";

        public readonly string OnExitTemplateWithDurationAndRequestId = "[{ClassName}, {MethodName}, {Id}] End Call. Took {Duration} ms.";

        public readonly string OnExitTemplateWithRequestId = "[{ClassName}, {MethodName}, {Id}] End Call.";

        public void OnExit(IJoinPoint joinPoint, Return @return, string requestId, long duration)
        {
            var log = Log.ForContext("Return", @return, true);
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                log.Debug(OnExitTemplateWithDurationAndRequestId, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, requestId, duration);
            }
            else
            {
                log.Debug(OnExitTemplateWithDuration, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, duration);
            }
        }

        public void OnEntry(IJoinPoint joinPoint, Argument[] arguments, string requestId)
        {
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                if (arguments != null && arguments.Length>0)
                {
                    var log = Log.ForContext("Arguments", arguments, true);
                    log.Debug(OnEntryTemplateWithRequestId, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, requestId);
                }
                else
                {
                    Log.Debug(OnEntryTemplateWithRequestId, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, requestId);
                }
            }
            else
            {
                if (arguments != null && arguments.Length > 0)
                {
                    var log = Log.ForContext("Arguments", arguments, true);
                    log.Debug(OnEntryTemplate, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name);
                }
                else
                {
                    Log.Debug(OnEntryTemplate, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name);
                }
            }
        }

        public void OnException(IJoinPoint joinPoint, string requestId, Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                Log.Error(ex, OnExceptionTemplateWithRequestId, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, requestId);
            }
            else
            {
                Log.Error(ex, OnExceptionTemplate, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name);
            }
        }

        public void OnExit(IJoinPoint joinPoint, string requestId, long duration)
        {
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                Log.Debug(OnExitTemplateWithDurationAndRequestId, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, requestId, duration);
            }
            else
            {
                Log.Debug(OnExitTemplateWithDuration, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, duration);
            }
        }

        public void OnExit(IJoinPoint joinPoint, Return @return, string requestId)
        {
            var log = Log.ForContext("Return", @return, true);
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                log.Debug(OnExitTemplateWithRequestId, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, requestId);
            }
            else
            {
                log.Debug(OnExitTemplate, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name);
            }
        }

        public void OnExit(IJoinPoint joinPoint, string requestId)
        {
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                Log.Debug(OnExitTemplateWithRequestId, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name, requestId);
            }
            else
            {
                Log.Debug(OnExitTemplate, joinPoint.TargetType.Name, joinPoint.MethodInfo.Name);
            }
        }
    }
}

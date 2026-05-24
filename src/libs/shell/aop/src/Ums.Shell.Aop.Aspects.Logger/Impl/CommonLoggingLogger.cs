using System;
using System.Text;
using Common.Logging;

namespace Ums.Shell.Aop.Aspects.Logger
{
    public class CommonLoggingLogger : ILogger
    {
        private readonly ILog _log;

        public readonly string OnExceptionTemplate = "[{class}.cs, {method}] Exception.";

        public readonly string OnExceptionTemplateWithRequestId = "[{class}.cs, {method}, {requestid}] Exception.";

        public readonly string OnEntryTemplateAndArguments = "[{class}.cs, {method}] Start Call. Arguments: {arguments}.";

        public readonly string OnEntryTemplateWithRequestIdAndArguments = "[{class}.cs, {method}, {requestid}] Start Call. Arguments: {arguments}.";

        public readonly string OnExitTemplateWithDurationAndReturn = "[{class}.cs, {method}] End Call. Took {took} ms. Return Value: {return}";

        public readonly string OnExitTemplateWithReturn = "[{class}.cs, {method}] End Call. Return Value: {return}";

        public readonly string OnExitTemplateWithRequestIdAndDurationAndReturn = "[{class}.cs, {method}, {requestid}] End Call. Took {took} ms. Return Value: {return}";

        public readonly string OnExitTemplateWithRequestIdAndReturn = "[{class}.cs, {method}, {requestid}] End Call. Return Value: {return}";

        private readonly ISerializer _serializer;

        public CommonLoggingLogger(ILog log, ISerializer serializer)
        {
            _log = log;

            _serializer = serializer;
        }

        public void OnExit(IJoinPoint joinPoint, Return @return, string requestId, long duration)
        {
            var returnValue = string.Empty;

            var template = OnExitTemplateWithDurationAndReturn;

            if (!string.IsNullOrWhiteSpace(requestId))
            {
                template = OnExitTemplateWithRequestIdAndDurationAndReturn;
            }

            var value = _serializer.Serialize(@return.Value);

            if (!string.IsNullOrWhiteSpace(value))
            {
                var s = string.Format("{0} = {1}", @return.Type, value);

                returnValue = string.Format("{0}, {1}", s, returnValue);
            }

            if (string.IsNullOrWhiteSpace(returnValue))
            {
                returnValue = "None";
            }

            var builder = new StringBuilder(template);
            builder.Replace("{class}", joinPoint.TargetType.Name);
            builder.Replace("{method}", joinPoint.MethodInfo.Name);
            builder.Replace("{took}", duration.ToString());
            builder.Replace("{return}", returnValue);
            builder.Replace("{requestid}", requestId);

            _log.Debug(builder.ToString());
        }

        public void OnEntry(IJoinPoint joinPoint, Argument[] arguments, string requestId)
        {
            var parameters = string.Empty;

            var template = OnEntryTemplateAndArguments;

            if(!string.IsNullOrWhiteSpace(requestId))
            {
                template = OnEntryTemplateWithRequestIdAndArguments;
            }

            if(arguments!=null && arguments.Length>0)
            {
                foreach (var argument in arguments)
                {
                    var value = _serializer.Serialize(argument.Value);

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var s = string.Format("{0} {1} = {2}", argument.Type, argument.Name, value);

                        parameters = string.Format("{0}, {1}", s, parameters);
                    }
                }
            }


            if (string.IsNullOrWhiteSpace(parameters))
            {
                parameters = "None";
            }

            var builder = new StringBuilder(template);
            builder.Replace("{class}", joinPoint.TargetType.Name);
            builder.Replace("{method}", joinPoint.MethodInfo.Name);
            builder.Replace("{arguments}", parameters);
            builder.Replace("{requestid}", requestId);

            _log.Debug(builder.ToString());
        }

        public void OnException(IJoinPoint joinPoint, string requestId, Exception ex)
        {
            var template = OnExceptionTemplate;

            if (!string.IsNullOrWhiteSpace(requestId))
            {
                template = OnExceptionTemplateWithRequestId;
            }

            var builder = new StringBuilder(template);
            builder.Replace("{class}", joinPoint.TargetType.Name);
            builder.Replace("{method}", joinPoint.MethodInfo.Name);
            builder.Replace("{requestid}", requestId);

            _log.Error(builder.ToString(), ex);
        }

        public void OnExit(IJoinPoint joinPoint, string requestId, long duration)
        {
            var returnValue = "None";

            var template = OnExitTemplateWithDurationAndReturn;

            if (!string.IsNullOrEmpty(requestId))
            {
                template = OnExitTemplateWithRequestIdAndDurationAndReturn;
            }

            var builder = new StringBuilder(template);
            builder.Replace("{class}", joinPoint.TargetType.Name);
            builder.Replace("{method}", joinPoint.MethodInfo.Name);
            builder.Replace("{took}", duration.ToString());
            builder.Replace("{return}", returnValue);
            builder.Replace("{requestid}", requestId);

            _log.Debug(builder.ToString());
        }

        public void OnExit(IJoinPoint joinPoint, Return @return, string requestId)
        {
            var returnValue = string.Empty;

            var template = OnExitTemplateWithReturn;

            if (!string.IsNullOrWhiteSpace(requestId))
            {
                template = OnExitTemplateWithRequestIdAndReturn;
            }

            var value = _serializer.Serialize(@return.Value);

            if (!string.IsNullOrWhiteSpace(value))
            {
                var s = string.Format("{0} = {1}", @return.Type, value);

                returnValue = string.Format("{0}, {1}", s, returnValue);
            }

            if (string.IsNullOrWhiteSpace(returnValue))
            {
                returnValue = "None";
            }

            var builder = new StringBuilder(template);
            builder.Replace("{class}", joinPoint.TargetType.Name);
            builder.Replace("{method}", joinPoint.MethodInfo.Name);
            builder.Replace("{return}", returnValue);
            builder.Replace("{requestid}", requestId);

            _log.Debug(builder.ToString());
        }

        public void OnExit(IJoinPoint joinPoint, string requestId)
        {
            var returnValue = "None";

            var template = OnExitTemplateWithReturn;

            if(!string.IsNullOrEmpty(requestId))
            {
                template = OnExitTemplateWithRequestIdAndReturn;
            }

            var builder = new StringBuilder(template);
            builder.Replace("{class}", joinPoint.TargetType.Name);
            builder.Replace("{method}", joinPoint.MethodInfo.Name);
            builder.Replace("{return}", returnValue);
            builder.Replace("{requestid}", requestId);

            _log.Debug(builder.ToString());
        }
    }
}

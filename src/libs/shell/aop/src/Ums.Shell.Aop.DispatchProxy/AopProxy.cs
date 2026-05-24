using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Ums.Shell.Aop.DispatchProxy
{
    public class AopProxy<TService, TImplementation> : System.Reflection.DispatchProxy
        where TService : class
        where TImplementation : class, TService
    {
        private static readonly ConcurrentDictionary<MethodInfo, MethodInfo> _methodCache = new ConcurrentDictionary<MethodInfo, MethodInfo>();

        private IAspectExecutor _executor;

        private TService _target;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var method = _methodCache.GetOrAdd(targetMethod, tm => typeof(TImplementation).GetMethod(
                tm.Name,
                tm.GetParameters().Select(parameter => parameter.ParameterType).ToArray()));

            var joinPoint = new JoinPoint
            {
                Arguments = args,

                MethodInfo = method,

                Return = targetMethod.ReturnType,

                TargetObject = _target,

                TargetType = typeof(TImplementation),

                ExecuteProxyInvocation = (jp =>
                {
                    var result = targetMethod.Invoke(_target, args);

                    jp.Return = result;
                }),
            };

            _executor.Execute(joinPoint);

            return joinPoint.Return;
        }

        public void Init(TService target, IAspectExecutor executor)
        {
            _executor = executor;

            _target = target;
        }
    }
}

using System.Collections.Concurrent;
using System.Reflection;

namespace Ums.Shell.Aop
{
    public abstract class AbstractAspect<T> : IAspect where T : AbstractAspectAttribute
    {
        private static readonly ConcurrentDictionary<MethodInfo, T> _attributeCache = new ConcurrentDictionary<MethodInfo, T>();

        protected T GetAttribute(IJoinPoint joinPoint)
        {
            return _attributeCache.GetOrAdd(joinPoint.MethodInfo, mi =>
            {
                var attributes = mi.GetCustomAttributes(typeof(T), true);
                return attributes.Length > 0 ? attributes[0] as T : default(T);
            });
        }

        protected virtual void Init(IJoinPoint joinPoint)
        {

        }

        public IAspect GetNext()
        {
            return _next;
        }

        public void SetNext(IAspect aspect)
        {
            _next = aspect;
        }

        private IAspect _next;

        public virtual void Apply(IJoinPoint joinPoint)
        {
            
        }

        public int GetOrder(IJoinPoint joinPoint)
        {
            var current = GetAttribute(joinPoint);

            return current?.Order ?? int.MaxValue;
        }
    }
}

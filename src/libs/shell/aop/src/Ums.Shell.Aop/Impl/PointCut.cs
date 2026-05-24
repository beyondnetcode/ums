using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Ums.Shell.Aop
{
    public class PointCut : IPointCut
    {
        private readonly ConcurrentDictionary<(MethodInfo, Type), bool> _cache = new ConcurrentDictionary<(MethodInfo, Type), bool>();

        public bool CanApply(IJoinPoint joinPoint, Type aspectType)
        {
            return _cache.GetOrAdd((joinPoint.MethodInfo, aspectType), key =>
            {
                var method = key.Item1;
                var aspect = key.Item2;

                if (aspect.BaseType != null && aspect.BaseType.IsGenericType)
                {
                    var attributeTypes = aspect.BaseType.GetGenericArguments();
                    if (attributeTypes.Length > 0)
                    {
                        var attributeType = attributeTypes.FirstOrDefault(x => typeof(AbstractAspectAttribute).IsAssignableFrom(x));
                        if (attributeType != null)
                        {
                            var attributes = method.GetCustomAttributes(attributeType, true);
                            return attributes.Length > 0;
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return true;
                }
            });
        }
    }
}

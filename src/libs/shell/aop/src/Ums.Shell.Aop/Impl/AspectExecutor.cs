using System;
using System.Linq;

namespace Ums.Shell.Aop
{
    public class AspectExecutor : IAspectExecutor
    {
        private readonly Type[] _types;

        private readonly IPointCut _pointCut;

        private readonly Func<Type, IAspect> _aspectFactory;

        public AspectExecutor(Type[] types, Func<Type, IAspect> aspectFactory, IPointCut pointCut)
        {
            _aspectFactory = aspectFactory;
            _pointCut = pointCut;
            _types = types;
        }

        public void Execute(IJoinPoint joinPoint)
        {
            var typesToApply = _types.Where(x => _pointCut.CanApply(joinPoint, x));

            if (typesToApply.Any())
            {
                var aspectsToApply = typesToApply.Select(_aspectFactory).OrderBy(x => x.GetOrder(joinPoint)).ToArray();

                var root = aspectsToApply[0];

                var aspect = root;

                for (var i = 1; i < aspectsToApply.Length; i++)
                {
                    aspect.SetNext(aspectsToApply[i]);

                    aspect = aspect.GetNext();
                }

                root.Apply(joinPoint);
            }
            else
            {
                joinPoint.Proceed();
            }
        }
    }
}

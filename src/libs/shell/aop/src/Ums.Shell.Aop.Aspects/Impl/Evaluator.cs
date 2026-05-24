using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Ums.Shell.Aop.Aspects
{
    public class Evaluator : IEvaluator
    {
        private readonly ConcurrentDictionary<(MethodInfo, string), Delegate> _cachedDelegates = new ConcurrentDictionary<(MethodInfo, string), Delegate>();

        public TOutput Evaluate<TOutput>(IJoinPoint joinPoint, string expression, TOutput errorValue = default(TOutput))
        {
            try
            {
                var key = (joinPoint.MethodInfo, expression);

                var compiledDelegate = _cachedDelegates.GetOrAdd(key, k =>
                {
                    var list = new List<ParameterExpression>();
                    var infos = k.Item1.GetParameters();

                    for (int i = 0; i < infos.Length; i++)
                    {
                        var info = infos[i];
                        list.Add(Expression.Parameter(info.ParameterType, info.Name));
                    }

                    var lambda = DynamicExpressionParser.ParseLambda(list.ToArray(), null, k.Item2);
                    return lambda.Compile();
                });

                var value = compiledDelegate.DynamicInvoke(joinPoint.Arguments);

                return (TOutput)Convert.ChangeType(value, typeof(TOutput));
            }
            catch (Exception)
            {
                return errorValue;
            }
        }
    }
}

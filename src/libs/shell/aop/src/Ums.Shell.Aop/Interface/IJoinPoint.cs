using System;
using System.Reflection;

namespace Ums.Shell.Aop
{
    public interface IJoinPoint
    {
        object[] Arguments { get; set; }

        MethodInfo MethodInfo { get; }

        object Return { get; set; }

        object TargetObject { get; }

        Type TargetType { get; }

        void Proceed();
    }
}

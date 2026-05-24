using System;

namespace Ums.Shell.Aop
{
    public interface IPointCut
    {
        bool CanApply(IJoinPoint joinPoint, Type type);
    }
}

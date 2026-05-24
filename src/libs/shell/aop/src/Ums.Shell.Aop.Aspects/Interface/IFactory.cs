using System;

namespace Ums.Shell.Aop.Aspects
{
    public interface IFactory<T>
    {
        T Create(Type type);
    }
}

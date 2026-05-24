using System;

namespace Ums.Shell.Aop.Aspects
{
    public class Factory<T> : IFactory<T> where T : class
    {
        private readonly Func<Type, T> _create;

        public Factory(Func<Type, T> create)
        {
            _create = create;
        }

        public T Create(Type type)
        {
            return _create(type);
        }
    }
}

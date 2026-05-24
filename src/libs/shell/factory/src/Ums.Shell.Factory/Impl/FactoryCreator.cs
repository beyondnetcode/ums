namespace Ums.Shell.Factory.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IFactoryCreator"/>.
    ///
    /// Receives a <c>Func&lt;Type, object&gt;</c> that knows how to instantiate a
    /// concrete type.  This keeps <see cref="FactoryCreator"/> free of any
    /// dependency on a service-locator or DI container.
    ///
    /// In a Microsoft.Extensions.DependencyInjection host the factory function
    /// is typically <c>sp.GetRequiredService</c>; in unit tests it can be a
    /// simple lambda over a pre-built dictionary or <c>Activator.CreateInstance</c>.
    /// </summary>
    public class FactoryCreator : IFactoryCreator
    {
        private readonly Func<Type, object> _instanceFactory;

        public FactoryCreator(Func<Type, object> instanceFactory)
        {
            ArgumentNullException.ThrowIfNull(instanceFactory, nameof(instanceFactory));
            _instanceFactory = instanceFactory;
        }

        public T Create<T>(Type type) where T : class
        {
            var instance = _instanceFactory(type)
                ?? throw new InvalidOperationException(
                    $"The instance factory returned null for type '{type.FullName}'.");

            if (instance is not T typed)
                throw new InvalidCastException(
                    $"Instance of '{instance.GetType().FullName}' is not assignable to '{typeof(T).FullName}'.");

            return typed;
        }
    }
}

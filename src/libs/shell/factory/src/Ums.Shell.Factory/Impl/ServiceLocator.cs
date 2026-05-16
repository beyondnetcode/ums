using Ums.Shell.Factory.Interfaces;
using Ums.Shell.Factory.Models;
using System.Diagnostics;

namespace Ums.Shell.Factory.Impl
{
    [DebuggerDisplay("Items: {_records.Count}")]
    public class ServiceLocator : IServiceLocator
    {
        public static IServiceLocator Current;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly IList<ServiceLocatorRecord> _records;

        public void Register(Type component, object instance)
        {
            _records.Add(new ServiceLocatorRecord(component, instance, component.FullName));
        }

        public void Register(Type component, object instance, string name)
        {
            _records.Add(new ServiceLocatorRecord(component, instance, name));
        }

        public ServiceLocator()
        {
            _records = new List<ServiceLocatorRecord>();
        }

        public TSource Resolve<TSource>() where TSource : class
        {
            foreach (var o in _records.Where(o => o.Component == typeof(TSource)))
            {
                return o.Implementation as TSource;
            }
            throw new ArgumentException($"It is not posible to get a instance of {typeof(TSource).Name}");
        }

        public TSource Resolve<TSource>(string name) where TSource : class
        {
            foreach (var o in _records.Where(o => o.Component == typeof(TSource) && o.Name == name))
            {
                return o.Implementation as TSource;
            }
            throw new ArgumentException($"It is not posible to get a instance of {typeof(TSource).Name}");
        }

        public TSource[] ResolveAll<TSource>() where TSource : class
        {
            return _records.Where(o => o.Component == typeof(TSource)).Select(x => x.Implementation as TSource).ToArray();
        }

        public object Resolve(Type service)
        {
            foreach (var o in _records.Where(o => o.Component == service))
            {
                return o.Implementation;
            }
            throw new ArgumentException($"It is not posible to get a instance of {service.Name}");
        }

        public object Resolve(Type service, string name)
        {
            foreach (var o in _records.Where(o => o.Component == service && o.Name == name))
            {
                return o.Implementation;
            }
            throw new ArgumentException($"It is not posible to get a instance of {service.Name}");
        }
    }
}

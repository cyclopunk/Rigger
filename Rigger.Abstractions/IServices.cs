using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rigger.Injection
{
    /// <summary>
    /// A lightweight IServiceProvider that will replace the ManagedTypeFactory within Rig
    /// </summary>
    public interface IServices : IServiceProvider, IDisposable, IAsyncDisposable
    {
        IEnumerable<ValidationError> Validate();

        IServices Add(Type lookupType, Type concreteType,
            ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient);

        IServices Add<TLookupType>(Type concreteType, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient);

        IServices Add<TLookupType, TConcreteType>(ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient);

        IServices Add(Type type, object instance);
        public IServices Add(Type lookupType, Func<IServices, Type, object> factory);

        /// <summary>
        /// Add a singleton instance.
        /// </summary>
        /// <typeparam name="TLookupType"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public IServices Add<TLookupType>(object instance);


        public IServices OfLifecycle(ServiceLifecycle serviceLifecycle);

        /// <summary>
        /// Get a service that is registered as the type provided
        /// </summary>
        /// <typeparam name="T">The type to lookup</typeparam>
        /// <returns>An instance of type T</returns>
        public T GetService<T>()
            where T : class
        {
            return (T) GetService(typeof(T));
        }

    }
}
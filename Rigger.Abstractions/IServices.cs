using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rigger.Injection
{
    public enum CallSiteType
    {
        Enumeration,
        Constructor,
        Field,
        Property,
        Factory,
        ServiceProvider,
        Scope,
        Method
    }
    /// <summary>
    /// A lightweight IServiceProvider that will replace the ManagedTypeFactory within Rig
    /// </summary>
    public interface IServices : IServiceProvider, IDisposable, IAsyncDisposable
    {
        IEnumerable<ValidationError> Validate();
        bool IsManaged(Type type);
        bool IsManaged<T>();

        //IServices Replace<T, R>() where R : T;
        //IServices Replace<T,R>(R instance) where R : T;
        IServices Add(Type lookupType, Type concreteType,
            ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient);

        IServices Add<TLookupType>(Type concreteType, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient);

        IServices Add<TLookupType, TConcreteType>(ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient);

        IServices Add(Type type, object instance);
        public IServices Add(Type lookupType, Func<IServices, object> factory, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Singleton);

        /// <summary>
        /// Add a singleton instance.
        /// </summary>
        /// <typeparam name="TLookupType"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public IServices Add<TLookupType>(object instance);


        public IServices OfLifecycle(params ServiceLifecycle[] serviceLifecycle);

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

        object GetService(Type serviceType, CallSiteType callsite = CallSiteType.Method);

        public void DisposeScope();

    }
}
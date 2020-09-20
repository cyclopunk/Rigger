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
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        IServices Add<TLookupType>(Type concreteType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        IServices Add<TLookupType, TConcreteType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        IServices Add(Type type, object instance);
        public IServices Add(Type lookupType, Func<IServices, object> factory, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton);

        /// <summary>
        /// Add a singleton instance.
        /// </summary>
        /// <typeparam name="TLookupType"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public IServices Add<TLookupType>(object instance);
        public IServices AddConditionalService(Type serviceType, Type instanceType, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton);

        public IEnumerable<ServiceDescription> List(Type type=null);

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
        public T GetService<T>(CallSiteType type)
            where T : class
        {
            return (T) GetService(typeof(T), type);
        }

        object GetService(Type serviceType, CallSiteType callsite = CallSiteType.Method);

        

        public void DisposeScope();

    }
}
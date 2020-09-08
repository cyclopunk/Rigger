using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.Injection.Defaults;
using Rigger.ManagedTypes.Resolvers;
using Rigger.Reflection;

namespace Rigger.Injection
{
    public class CallSite
    {
        private Type serviceType;
        private CallSiteType enumeration;

        public CallSite(Type serviceType, CallSiteType enumeration)
        {
            this.serviceType = serviceType;
            this.enumeration = enumeration;
        }

        public CallSiteType Type { get; set; }
        public Type LookupType { get; set; }
        public Type ImplementationType { get; set; }

        public bool HasSameLookupType(Type type)
        {
            return type == LookupType;
        }

        public override bool Equals(object obj)
        {
            if (obj is CallSite cs)
            {
                return cs.Type == this.Type && LookupType == cs.LookupType;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, LookupType);
        }
    }
    public class ServiceResolution
    {
        public object Instance { get; set; }
    }

    public interface IServiceResolver
    {
        public object Resolve();
    }

    public class EnumerableServiceResulver : IServiceResolver
    {
        internal Services services;

        internal Type LookupType;

        internal IEnumerable<ServiceDescription> Descriptions;

        internal object ResolvedService;

        public object Resolve()
        {
            if (ResolvedService != null)
            {
                return ResolvedService;
            }

            var type = typeof(List<>).MakeGenericType(LookupType);

            var listActivator = new ExpressionActivator(type);

            var list = listActivator.Activate();

            var methodActivator = new SingleParameterMethodAccessor(type.GetMethod("Add"));

            Descriptions.ForEach(o => {
                methodActivator.Invoke(list, services.GetService(o.ImplementationType, CallSiteType.Enumeration));
            });

            ResolvedService = list;

            return list;
        }
    }
    public class ConditionalResolver : IServiceResolver
    {
        public object Resolve()
        {
            return null;
        }
    }

    /// <summary>
    /// A lightweight IServiceProvider that will replace the ManagedTypeFactory within Rig
    /// </summary>
    public class Services : IServices
    {

        private readonly IDictionary<CallSite, IServiceResolver> _resolutions = new Dictionary<CallSite, IServiceResolver>();
        private readonly ConcurrentBag<ServiceDescription> services = new ConcurrentBag<ServiceDescription>();

        private bool _disposedValue;

        public Services()
        {

        }

        public Services(IEnumerable<ServiceDescription> parent)
        {
            parent.ForEach(services.Add);
        }
        public Services(IEnumerable<ServiceDescription> parent, IEnumerable<IServiceInstance> instances)
        {
            parent.ForEach(services.Add);
        }

        /// <summary>
        /// Main Method for registering a new service.
        ///  
        /// </summary>
        /// <param name="lookupType"></param>
        /// <param name="concreteType"></param>
        /// <param name="serviceLifecycle"></param>
        /// <returns></returns>
        public IServices Add(Type serviceType, Type concreteType, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient)
        {
            services.Add(new ServiceDescription
            {
                ServiceType = serviceType,
                ImplementationType = concreteType,
                LifeCycle = serviceLifecycle
            });

            return this;
        }

        public IServices Add<TLookupType>(Type concreteType, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient)
        {
            Add(typeof(TLookupType), concreteType, serviceLifecycle);

            return this;
        }

        public IServices Add<TLookupType, TConcreteType>(ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient)
        {
            Add(typeof(TLookupType), typeof(TConcreteType), serviceLifecycle);

            return this;
        }

        public IServices Add(Type type, object instance)
        {
           


            return this;
        }
        /// <summary>
        /// Add a service factory
        /// </summary>
        /// <param name="lookupType">The abstract service type</param>
        /// <param name="factory">A factory that will create instances of the object</param>
        /// <param name="lifecycle">The lifecycle of the object, defaults to singleton</param>
        /// <returns></returns>
        public IServices Add(Type lookupType, Func<IServices, object> factory, ServiceLifecycle lifecycle = ServiceLifecycle.Singleton)
        {
            services.Add(new ServiceDescription
            {
                ServiceType = lookupType,
                ImplementationType = null,
                Factory = factory,
                LifeCycle = lifecycle
            });

            return this;
        }
        /// <summary>
        /// Add a singleton instance.
        /// </summary>
        /// <typeparam name="TLookupType"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public IServices Add<TLookupType>(object instance)
        {
            Add(typeof(TLookupType), instance);

            return this;
        }

        public IEnumerable<ServiceDescription> GetDescription(Type type)
        {
            return services.Where(o => o.ServiceType == type);
        }
        // create a new service provider with specified instances of the lifecycles specified.
        // TODO this will probably be slow, fix it.
        public IServices OfLifecycle(params ServiceLifecycle[] serviceLifecycle)
        {
            var descriptions = services.Where(o => serviceLifecycle.Contains(o.LifeCycle)).ToList();

            return new Services(services);
        }
        public ValueTask DisposeAsync()
        {
            this.Dispose();

            return new ValueTask(Task.CompletedTask);
        }

        public IEnumerable<ServiceDescription> Get(Type type)
        {
            return services.Where(o => o.ServiceType == type);
        }

        public IEnumerable<ServiceDescription> Get<T>()
        {
            return Get(typeof(T));
        }


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

   
        /// <summary>
        /// Get the service instance for a description
        /// </summary>
        /// <param name="description"></param>
        /// <param name="lookupType"></param>
        /// <param name="overrideImplType">Type that can override the implementation type</param>
        /// <returns></returns>
        internal IServiceInstance GetServiceInstance(ServiceDescription description, Type lookupType, Type overrideImplType=null)
        {
            return description.LifeCycle switch
            {
                ServiceLifecycle.Singleton => new SingletonServiceInstance
                    {InstanceType = overrideImplType ?? description.ImplementationType, LookupType = lookupType, ServiceType = description.ServiceType}.AddServices(this),
                ServiceLifecycle.Scoped => new ScopedServiceInstance
                    {InstanceType = overrideImplType ?? description.ImplementationType, LookupType = lookupType, ServiceType = description.ServiceType}.AddServices(this),
                ServiceLifecycle.Thread =>new ThreadServiceInstance
                    {InstanceType = description.ImplementationType, LookupType = lookupType, ServiceType = description.ServiceType}.AddServices(this),
                _ => new DefaultServiceInstance {InstanceType = overrideImplType ?? description.ImplementationType, LookupType = lookupType, ServiceType = description.ServiceType}.AddServices(this)
            };
        }

        /// <summary>
        /// This method will return an enumerable
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal object GetEnumerable(Type serviceType)
        {
            // an enumerbale was requested.

            var enumerable = GetService(serviceType, CallSiteType.Enumeration);

            return enumerable;
        }


        /// <summary>
        /// Get a service 
        /// </summary>
        /// <param name="serviceType">The type that will be created</param>
        /// <returns></returns>
        public object GetService(Type serviceType, CallSiteType callsite = CallSiteType.Method)
        {

            if (serviceType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(serviceType.GetGenericTypeDefinition()))
            {
                return GetEnumerable(serviceType);
            }

            switch (callsite)
            {
                case CallSiteType.Enumeration:
                    // this is coming from an enumeration, so we know the type is an implementation type.


                    break;
                default:
                        _resolutions.GetOrPut(new CallSite (serviceType, callsite), () => )
                    break;
            }

            return null;
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                services.Clear();
            }
        }

        public void DisposeScope()
        {
          
        }

        public bool IsManaged(Type type)
        {
            return services.Any( o => o.ServiceType == type);
        }

        public bool IsManaged<T>()
        {
            return IsManaged(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            return GetService(serviceType, CallSiteType.Method);
        }

        public IEnumerable<ValidationError> Validate()
        {
            /*return _descriptionMap.Values.SelectMany(o => o.Where(x => !x.IsValid())
               .Select(s => new ValidationError { Error = $"Invaild service {s.ServiceType} for {s.ImplementationType}" }));*/

            return new List<ValidationError>(); // todo redo

        }
    }
}
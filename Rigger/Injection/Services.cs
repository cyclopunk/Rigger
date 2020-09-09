using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rigger.Attributes;
using Rigger.Exceptions;
using Rigger.Extensions;
using Rigger.Injection.Defaults;
using Rigger.ManagedTypes.Resolvers;

namespace Rigger.Injection
{
    /// <summary>
    /// A lightweight IServiceProvider that will replace the ManagedTypeFactory within Rig
    /// </summary>
    public class Services : IServices
    {

        private readonly IDictionary<CallSite, IServiceResolver> _resolutions = new Dictionary<CallSite, IServiceResolver>();
        private readonly ConcurrentQueue<ServiceDescription> services = new ConcurrentQueue<ServiceDescription>();
        private readonly ConcurrentDictionary<Type, object> singletons = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, ExpressionTypeResolver> conditionalServices = new ConcurrentDictionary<Type, ExpressionTypeResolver>();

        private bool _disposedValue;

        public Services()
        {

        }

        internal class SingletonContainer : List<object>
        {

        }

        /// <summary>
        /// Main Method for registering a new service.
        ///  
        /// </summary>
        /// <param name="lookupType"></param>
        /// <param name="concreteType"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public IServices Add(Type serviceType, Type concreteType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            services.Enqueue(new ServiceDescription
            {
                ServiceType = serviceType,
                ImplementationType = concreteType,
                Lifetime = serviceLifetime
            });

            if (ServiceLifetime.Singleton == serviceLifetime)
            {
                singletons.TryAdd(serviceType, null);
            }

            return this;
        }

        public IServices Add<TLookupType>(Type concreteType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            Add(typeof(TLookupType), concreteType, serviceLifetime);

            return this;
        }

        public IServices Add<TLookupType, TConcreteType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            Add(typeof(TLookupType), typeof(TConcreteType), serviceLifetime);

            return this;
        }

        public IServices Add(Type type, object instance)
        {
            var description = new ServiceDescription
            {
                ServiceType = type,
                ImplementationType = instance.GetType(),
                Factory = null,
                Lifetime = ServiceLifetime.Singleton
            };

            services.Enqueue(description);

            if (instance is IServiceAware sa)
            {
                sa.Services = this;
            }

            singletons.AddOrUpdate(type, instance, (k, v) =>
            {
                object o = v;

                if (o is SingletonContainer i)
                {
                    i.Add(instance);
                }
                else
                {
                    o = new SingletonContainer {instance, v};
                }

                return o;
            });

            return this;
        }
        /// <summary>
        /// Add a service factory
        /// </summary>
        /// <param name="lookupType">The abstract service type</param>
        /// <param name="factory">A factory that will create instances of the object</param>
        /// <param name="lifetime">The lifetime of the object, defaults to singleton</param>
        /// <returns></returns>
        public IServices Add(Type lookupType, Func<IServices, object> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            services.Enqueue(new ServiceDescription
            {
                ServiceType = lookupType,
                ImplementationType = null,
                Factory = factory,
                Lifetime = lifetime
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

        public IServices AddConditionalService(Type serviceType, Type instanceType,
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            ExpressionTypeResolver resolver = conditionalServices.GetOrAdd(serviceType, (k) => new ExpressionTypeResolver().AddServices(this));
            var expression = instanceType.GetCustomAttribute<ConditionAttribute>().Expression;

            resolver.AddType(expression, instanceType);

            return this;
        }

        public IEnumerable<ServiceDescription> List(Type type=null)
        {
            return services.Where(i => type == null || i.ServiceType == type);
        }

        public IEnumerable<ServiceDescription> GetDescription(Type type)
        {
            return services.Where(o => o.ServiceType == type);
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
        public T GetService<T>(CallSiteType type)
            where T : class
        { 
            return (T) GetService(typeof(T), type);
        }

        /// <summary>
        /// Get a service 
        /// </summary>
        /// <param name="serviceType">The type that will be created</param>
        /// <param name="callsite">The site where this service was instantiated from</param>
        /// <returns></returns>
        public object GetService(Type serviceType, CallSiteType callsite)
        {
            
            // generic handling

            if (serviceType.IsGenericType)
            {
                var genericType = serviceType.GetGenericTypeDefinition();

                if (typeof(IEnumerable<>).IsAssignableFrom(genericType))
                {
                    var typeParam = serviceType.GetGenericArguments().First();

                    singletons.TryGetValue(typeParam, out var sing);

                    return _resolutions.GetOrPut(new CallSite(serviceType, callsite),
                        () => new EnumerableServiceResolver(this, serviceType, sing, this.services.Where(o => o.ServiceType == typeParam))).Resolve();
                }

                if (GetDescription(genericType).FirstOrDefault() != null)
                {
                    return _resolutions.GetOrPut(new CallSite(serviceType, callsite),
                        () => new OpenGenericResolver(this, serviceType)).Resolve();
                }
            }

            // conditionals

            if (conditionalServices.ContainsKey(serviceType))
            {
                return _resolutions.GetOrPut(new CallSite(serviceType, callsite), () => new ConditionalResolver(this, serviceType, conditionalServices[serviceType])).Resolve();
            }

            // singletons

            var isSingleton = singletons.ContainsKey(serviceType);

            if (isSingleton)
            {
                //var sinstance = singletons[serviceType];
                singletons.TryGetValue(serviceType, out var sinstance);

                var sinstances = singletons.Where(o => o.Key == serviceType);

                if (sinstance != null)
                {
                    return sinstance;
                }
            }

            var resolver = callsite switch
            {
                CallSiteType.Scope => new DefaultResolver(serviceType) {Services = this}, // don't save the resolver for a scoped call
                CallSiteType.Enumeration => _resolutions.GetOrPut(new CallSite(serviceType, callsite),
                    () => new ImplementationTypeResolver(this, serviceType)),
                CallSiteType.ServiceProvider => _resolutions.GetOrPut(new CallSite(serviceType, callsite),
                    () => new ServiceProviderResolver(serviceType) {Services = this}),
                _ => _resolutions.GetOrPut(new CallSite(serviceType, callsite),
                    () => new DefaultResolver(serviceType) {Services = this})
            };

            var instance = resolver.Resolve();

            if (isSingleton)
            {
                singletons.TryUpdate(serviceType, instance, null);
            }

            if (instance == null)
            {

            }

            return instance;
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
            return GetService(serviceType, CallSiteType.ServiceProvider);
        }

        public IEnumerable<ValidationError> Validate()
        {
            /*return _descriptionMap.Values.SelectMany(o => o.Where(x => !x.IsValid())
               .Select(s => new ValidationError { Error = $"Invaild service {s.ServiceType} for {s.ImplementationType}" }));*/

            return new List<ValidationError>(); // todo redo

        }
    }
}
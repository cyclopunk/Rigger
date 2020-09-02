using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheCommons.Core.Extensions;
using TheCommons.Core.Reflection;
using TheCommons.Forge.ManagedTypes.Lightweight.Defaults;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    /// <summary>
    /// A lightweight IServiceProvider that will replace the ManagedTypeFactory within Crucible
    /// </summary>
    public class Services : IServiceProvider, IDisposable, IAsyncDisposable
    {
        private readonly IDictionary<Type, ServiceDescription> _descriptionMap = new Dictionary<Type, ServiceDescription>();
        private readonly IDictionary<Type, IServiceInstance> _instanceMap = new Dictionary<Type, IServiceInstance>();
        private bool _disposedValue;

        public Services()
        {

        }
        public IEnumerable<ValidationError> Validate()
        {
            return _descriptionMap.Values
                .Where(o => !ServiceDescriptionExtensions.IsValid(o))
                .Select( s => new ValidationError { Error = $"Invaild service {s.ServiceType} for {s.ImplementationType}" });
        }
        public Services(IEnumerable<ServiceDescription> parent)
        {
            foreach (var d in parent)
            {
                _descriptionMap[d.ServiceType] = d;
            }
        }
        public Services(IEnumerable<ServiceDescription> parent, IEnumerable<IServiceInstance> instances)
        {
            foreach (var d in parent)
            {
                _descriptionMap[d.ServiceType] = d;
            }
            foreach (var d in instances)
            {
                _instanceMap[d.InstanceType] = d;
            }
        }

        public Services Add(Type lookupType, Type concreteType, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient)
        {

            if (_descriptionMap.ContainsKey(lookupType))
            {
                
                var sd = _descriptionMap[lookupType];
                sd.ExtraTypes.Add(concreteType);
            }
            else
            {
                _descriptionMap[lookupType] = new ServiceDescription
                {
                    ServiceType = lookupType,
                    ImplementationType = concreteType,
                    LifeCycle = serviceLifecycle
                };
            }

            return this;
        }

        public Services Add<TLookupType>(Type concreteType, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient)
        {
            Add(typeof(TLookupType), concreteType, serviceLifecycle);

            return this;
        }

        public Services Add<TLookupType, TConcreteType>(ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient)
        {
            Add(typeof(TLookupType), typeof(TConcreteType), serviceLifecycle);

            return this;
        }

        public Services Add(Type type, object instance)
        {
            _descriptionMap.Add(type, new ServiceDescription
            {
                ServiceType = type,
                ImplementationType = instance.GetType(),
                LifeCycle = ServiceLifecycle.Singleton
            });
            _instanceMap.Add(type, new SingletonServiceInstance(instance).AddServices(this));

            return this;
        }
        /// <summary>
        /// Add a singleton instance.
        /// </summary>
        /// <typeparam name="TLookupType"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Services Add<TLookupType>(object instance)
        {
            Add(typeof(TLookupType), instance);

            return this;
        }

        public ServiceDescription GetDescription(Type type)
        {
            return _descriptionMap[type];
        }
        public Services OfLifecycle(ServiceLifecycle serviceLifecycle)
        {
            return new Services(_descriptionMap.Values.Where(i => i.LifeCycle == serviceLifecycle));
        }
        public ValueTask DisposeAsync()
        {
            this.Dispose();

            return new ValueTask(Task.CompletedTask);
        }

        public ServiceDescription Get(Type serviceType)
        {
            _descriptionMap.TryGetValue(serviceType, out var service);

            return service;
        }

        public ServiceDescription Get<T>()
        {
            _descriptionMap.TryGetValue(typeof(T), out var service);

            return service;
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
        /// TODO Add thread type
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        internal IServiceInstance GetServiceInstance(ServiceDescription desc)
        {
            return desc.LifeCycle switch
            {
                ServiceLifecycle.Singleton => new SingletonServiceInstance
                    {InstanceType = desc.ImplementationType}.AddServices(this),
                ServiceLifecycle.Thread =>new ThreadServiceInstance
                    {InstanceType = desc.ImplementationType}.AddServices(this),
                _ => new DefaultServiceInstance {InstanceType = desc.ImplementationType}.AddServices(this)
            };
        }

        /// <summary>
        /// This method will return an enumerable
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal object GetEnumerable(Type serviceType)
        {
            var type = serviceType.GetGenericArguments().FirstOrDefault();

            if (type == null)
            {
                return null;
            }

            var activator = new ExpressionActivator(typeof(List<>).MakeGenericType(type));

            var list = activator.Activate();

            var mi = new SingleParameterMethodAccessor(list.GetType(), "Add");

            var desc = _descriptionMap[type];
            
            desc?.AllTypes()?.ForEach(o =>
            {
                // create an instance activator for all types if one doesn't exist
                var instance = _instanceMap.GetOrPut(o, () => GetServiceInstance(desc));
                
                // add it to the list using the method accessor

                mi.Invoke(list, instance?.Get());
            });

            return list;
        }

        /// <summary>
        /// Get a service 
        /// </summary>
        /// <param name="serviceType">The type that will be created</param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            _instanceMap.TryGetValue(serviceType, out var service);

            if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return GetEnumerable(serviceType);
            }

            if (service == null)
            {
                // Deal with a type that we have an open generic
                // description of (like ILogger<>)
                if (serviceType.IsConstructedGenericType 
                    && !_descriptionMap.ContainsKey(serviceType))
                {
                    var generic = serviceType.GetGenericTypeDefinition();
                    
                    _descriptionMap.TryGetValue(generic, out var openType);

                    if (openType != null)
                    {
                        var makeType = openType.ImplementationType.MakeGenericType(serviceType.GetGenericArguments());

                        // cache constructed type

                        Add(serviceType, makeType, openType.LifeCycle);
                    }
                    else
                    {
                        return null;
                    }
                }

                _descriptionMap.TryGetValue(serviceType, out var desc);

                if (desc != null)
                {
                    IServiceInstance instance = GetServiceInstance(desc);

                    _instanceMap.Add(serviceType, instance);

                    return instance.Get();
                }
            }

            var i = service?.Get();

            // if the instance is service aware, inject the services
            if (i is IServiceAware sa && sa.Services == null)
            {
                sa.AddServices(this);
            }

            return  i;
        }

        public void Dispose()
        {
            if (!_disposedValue)
            {
                _descriptionMap.Clear();

                _instanceMap.Values.ForEach(i =>
                {
                    if (i is IDisposable d)
                    {
                        d.Dispose();
                    }
                });

                _instanceMap.Clear();

                _disposedValue = true;
            }
        }

    }
}
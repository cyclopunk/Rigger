using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rigger.Extensions;
using Rigger.Injection.Defaults;
using Rigger.Reflection;

namespace Rigger.Injection
{
    /// <summary>
    /// A lightweight IServiceProvider that will replace the ManagedTypeFactory within Rig
    /// </summary>
    public class Services : IServices
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
                Type st = _descriptionMap
                    .FirstOrDefault(kvp => kvp.Value.ImplementationType == d.InstanceType)
                    .Value?.ServiceType;
                if (st != null)
                    _instanceMap[st] = d;
            }
        }

        public IServices Add(Type lookupType, Type concreteType, ServiceLifecycle serviceLifecycle = ServiceLifecycle.Transient)
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
            if (_descriptionMap.ContainsKey(type))
            {

                var sd = _descriptionMap[type];
                
                sd.ExtraInstances.Add(instance);

             

                return this;
            }

            _descriptionMap.Add(type, new ServiceDescription
            {
                ServiceType = type,
                ImplementationType = instance.GetType(),
                LifeCycle = ServiceLifecycle.Singleton,
                ExtraInstances = new List<object>() { instance }
            });

            //_instanceMap.Add(type, new SingletonServiceInstance(instance) { LookupType = type, ServiceType = type}.AddServices(this));

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
            _descriptionMap.Add(lookupType, new ServiceDescription
            {
                ServiceType = lookupType,
                ImplementationType = lookupType,
                Factory = factory,
                LifeCycle = lifecycle
            });;

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

        public IServices Remove<TLookupType>()
        {
            _descriptionMap.Remove(typeof(TLookupType));
            _instanceMap.Remove(typeof(TLookupType));

            return this;
        }
        public IServices Remove(Type type)
        {
            _descriptionMap.Remove(type);
            _instanceMap.Remove(type);

            return this;
        }

        public ServiceDescription GetDescription(Type type)
        {
            return _descriptionMap[type];
        }
        // create a new service provider with specified instances of the lifecycles specified.
        // TODO this will probably be slow, fix it.
        public IServices OfLifecycle(params ServiceLifecycle[] serviceLifecycle)
        {
            var descriptions = _descriptionMap.Values.Where(o => serviceLifecycle.Contains(o.LifeCycle)).ToList();

            var instances = _instanceMap.Where(kvp => descriptions
                    .Any(o => o.ServiceType == kvp.Value.ServiceType))
                    .Select(s => s.Value);

            return new Services(_descriptionMap.Values, instances);
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
        /// Get the service instance for a description
        /// </summary>
        /// <param name="description"></param>
        /// <param name="lookupType"></param>
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
            

            var type = serviceType.GetGenericArguments().FirstOrDefault();

            Console.WriteLine($"Getting enumerable for {type}");

            if (type == null)
            {
                return null;
            }

          
            var activator = new ExpressionActivator(typeof(List<>).MakeGenericType(type));

            var list = activator.Activate();

            if (!_descriptionMap.ContainsKey(type))
            {
                Console.WriteLine($"Could not find any registered types for {type.Name} {type.GenericTypeArguments}");
                return list;
            }

            var mi = new SingleParameterMethodAccessor(list.GetType(), "Add");

            var desc = _descriptionMap[type];

            if (desc.Factory != null)
            {
                object i = desc.Factory(this);

                mi.Invoke(list, i);
                
                Console.WriteLine($"- Added instance from Factory {i}");
            }
            
            desc?.AllTypes()?.ForEach(o =>
            {
                try
                {
                    if (o.IsInterface)
                    {
                        return;
                    }

                    // create an instance activator for all types if one doesn't exist
                    var instance = _instanceMap.GetOrPut(o, () => GetServiceInstance(desc, type, o));

                    // add it to the list using the method accessor
                    var i = instance?.Get();

                    mi.Invoke(list, instance?.Get());

                //    Console.WriteLine($"- Added instance from ServiceInstance {i}");
                
                } catch (Exception)
                {
                }
            });

            desc?.ExtraInstances.ForEach(o => mi.Invoke(list, o));


            Console.WriteLine($"Returning for {serviceType.Name}");

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
                    object newInstance = null;

                    if (desc.Factory != null)
                        return desc.Factory(this);

                    if (desc.ExtraInstances.Count > 0)
                    {
                        newInstance = desc.ExtraInstances.First();
                    }
                    else
                    {

                        IServiceInstance instance = GetServiceInstance(desc, serviceType);

                        _instanceMap.Add(serviceType, instance);
                        newInstance = instance.Get();
                    }


                    if (newInstance is IServiceAware svc && svc.Services == null)
                    {
                        svc.AddServices(this);
                    }


                    return newInstance;
                }
            }

            // has an instance already.


           // Console.WriteLine($"Get existing instance: {serviceType}");

            var i = service?.Get();
            
            /*if (i == null)
            {
                Console.WriteLine($"Could not get instance for {serviceType}");
            }*/

            // if the instance is service aware, inject the services
            if (i is IServiceAware sa && sa.Services == null)
            {
                sa.AddServices(this);
            }

            return  i;
        }

        /// <summary>
        /// Replace a service 
        /// </summary>
        /// <typeparam name="T">The service to replace</typeparam>
        /// <typeparam name="R">The concrete type of the service</typeparam>
        public IServices Replace<T, R>() where R : T
        {
            if (_descriptionMap.ContainsKey(typeof(T)))
            {
                Remove<T>();
            }
            Add<T, R>();

            return this;
        }
        public IServices Replace<T, R>(R instance) where R : T
        {
            if (_descriptionMap.ContainsKey(typeof(T)))
            {
                Remove<T>();
            }

            Add<T>(instance);

            return this;
        }
        public void Dispose()
        {
            if (!_disposedValue)
            {
                _descriptionMap.Clear();

                _instanceMap.Values.ForEach(i =>
                {
                    // the instance map may hold a reference to this object
                    // ignore it if it does.
                    if (i is IDisposable d)
                    {
                        d.Dispose();
                    }
                });

                _instanceMap.Clear();

                _disposedValue = true;
            }
        }

        public void DisposeScope()
        {
            _instanceMap.Values.ForEach(i =>
            {
                // the instance map may hold a reference to this object
                // ignore it if it does.
                if (i.Get() != this)
                {
                    if (i is ScopedServiceInstance d)
                    {
                        d.Dispose();
                    }
                }
            });
            _disposedValue = true;
            _descriptionMap.Clear();
            _instanceMap.Clear();
        }

        public bool IsManaged(Type type)
        {
            return _descriptionMap.ContainsKey(type);
        }

        public bool IsManaged<T>()
        {
            return _descriptionMap.ContainsKey(typeof(T));
        }
    }
}
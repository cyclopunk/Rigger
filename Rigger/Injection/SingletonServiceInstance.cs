using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.Reflection;

namespace Rigger.Injection
{
    public class ScopedServiceInstance : SingletonServiceInstance
    {

    }

    public class SingletonServiceInstance : IServiceInstance, IServiceAware
    {
        private List<ZeroParameterMethodAccessor> _cache;
        public Type ServiceType { get; set; }
        public Type InstanceType { get; set; }
        public IServices Services { get; set; }
        public Type LookupType { get; set; }

        private object _instanceInternal;

        public SingletonServiceInstance()
        {

        }

        internal SingletonServiceInstance(object instance)
        {
            InstanceType = instance.GetType();
            _instanceInternal = instance;
        }
        public object Instance
        {
            get
            {

                if (_instanceInternal == null)
                {
                    var instanceFactory = Services.GetService<IInstanceFactory>();

                    if (instanceFactory == null)
                    {
                        Services.GetService<ILogger<SingletonServiceInstance>>()
                            ?.LogWarning("IInstanceFactory not registered, using slow Activator");
                    }

                    _instanceInternal ??= instanceFactory?.Make(InstanceType) ?? (InstanceType.HasServiceConstructor() ? Activator.CreateInstance(InstanceType, Services) : Activator.CreateInstance(InstanceType));
                    
                    InstanceType
                        .MethodsWithAttribute(typeof(OnCreateAttribute)).ForEach(o =>
                        {
                            new ZeroParameterMethodAccessor(o).Invoke(_instanceInternal);
                        });
                    
                    return _instanceInternal;
                }
                
                return _instanceInternal;
                
            }
        }

        public object Get()
        {
            return Instance;
        }

        public void Dispose()
        {
            if (_instanceInternal is IDisposable d)
            {
                d.Dispose();
            }

            // explicit for now
            Services = null;
            InstanceType = null;
        }
    }
}
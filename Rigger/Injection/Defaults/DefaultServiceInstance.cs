using System;
using System.Collections.Generic;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.Reflection;

namespace Rigger.Injection.Defaults
{
    public class DefaultServiceInstance : IServiceInstance, IServiceAware
    {
        
        private List<ZeroParameterMethodAccessor> _cache;       
        public void Dispose()
        {
            Services = null;
            InstanceType = null;
        }

        public IServices Services { get; set; }
        public Type LookupType { get; set; }
        public Type ServiceType { get; set; }
        public Type InstanceType { get; set; }

        public object Get()
        {
            object instance;

            // loop detection
            if (ServiceType == typeof(IInstanceFactory))
            {
                instance = Activator.CreateInstance(InstanceType);

                if (instance is IServiceAware factory)
                {
                    factory.Services = Services;
                }

            }
            else
            {
                var instanceFactory = Services.GetService<IInstanceFactory>();
                // default is transient
                instance = instanceFactory?.Make(InstanceType) ?? Activator.CreateInstance(InstanceType);
            }

            if (_cache == null)
            {
                InstanceType
                    .MethodsWithAttribute(typeof(OnCreateAttribute)).ForEach(o =>
                    {
                        var accessor = new ZeroParameterMethodAccessor(o);
                        
                        _cache.Add(accessor);

                        accessor.Invoke(instance);
                    });
            }
            else
            {
                _cache.ForEach(o => o.Invoke(instance));
            }

            return instance;
        }
    }
}
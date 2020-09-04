using System;
using System.Collections.Generic;
using System.Threading;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.ManagedTypes.Implementations;
using Rigger.Reflection;

namespace Rigger.Injection
{
    /// <summary>
    /// Service Instance that uses a ThreadLocal
    /// </summary>
    public class ThreadServiceInstance : IServiceInstance, IServiceAware
    {          
        private ThreadLocal<object> _threadLocalFactory;
        public IServices Services { get; set; }
        public Type LookupType { get; set; }
        public Type ServiceType { get; set; }
        public Type InstanceType { get; set; }

        public object Instance => _threadLocalFactory.Value;


        public object Get()
        {
            _threadLocalFactory = new ThreadLocal<object>(() =>
            {
                var instanceFactory = Services.GetService<IInstanceFactory>();

                var instance = instanceFactory.Make(InstanceType) ?? Activator.CreateInstance(InstanceType);
                
                InstanceType
                    .MethodsWithAttribute(typeof(OnCreateAttribute)).ForEach(o =>
                    {
                        new ZeroParameterMethodAccessor(o).Invoke(instance);
                        //_cache.Add(new ZeroParameterMethodAccessor(o));
                    });

                return instance;
            });


            return _threadLocalFactory.Value;
        }

        public void Dispose()
        {
            _threadLocalFactory?.Dispose();
        }
    }
}
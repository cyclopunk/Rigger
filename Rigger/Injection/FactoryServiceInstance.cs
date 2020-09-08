using System;
using System.Threading;

namespace Rigger.Injection
{
    public class FactoryServiceInstance : IServiceInstance, IServiceAware
    {
        private ThreadLocal<object> _threadLocalFactory;
        private ServiceDescription description;

        public IServices Services { get; set; }
        public Type LookupType { get; set;  }
        public Type ServiceType { get; set;  }
        public Type InstanceType { get; set;  }

        private object _instanceInternal;

        internal FactoryServiceInstance(ServiceDescription description)
        {
            this.description = description;
            this.InstanceType = this.description.ImplementationType;
        }

        private object MakeSingletonInstance()
        {
            if (_instanceInternal == null)
            {

                _instanceInternal ??= description.Factory(this.Services);

                return _instanceInternal;
            }

            return _instanceInternal;
        }
        public object MakeThreadInstance()
        {
            _threadLocalFactory = new ThreadLocal<object>(() => description.Factory(Services));


            return _threadLocalFactory.Value;
        }

        public object Instance
        {
            get
            {
                return description.Lifetime switch
                {
                    ServiceLifetime.Thread => MakeThreadInstance(),
                    ServiceLifetime.Singleton => MakeSingletonInstance(),
                    _ => description.Factory(Services)
                };
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
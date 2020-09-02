using System;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    public class SingletonServiceInstance : IServiceInstance, IServiceAware
    {
        public Type InstanceType { get; set; }

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

                    _instanceInternal ??= instanceFactory.Make(InstanceType) ?? Activator.CreateInstance(InstanceType);

                    return _instanceInternal;
                }
                
                return _instanceInternal;
                
            }
        }

        public Services Services { get; set; }

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
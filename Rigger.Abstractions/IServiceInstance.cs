using System;

namespace Rigger.Injection
{
    public interface IServiceInstance : IDisposable
    {
        public IServices Services { get; set; }
        public Type LookupType { get; set; }
        public Type ServiceType { get; set; }
        public Type InstanceType { get; set; }
        public object Get()
        {
            // no loops
            if (ServiceType == typeof(IInstanceFactory))
            {
                var instance = Activator.CreateInstance(InstanceType);

                if (instance is IServiceAware factory)
                {
                    factory.Services = Services;
                }

                return instance;
            }
            var instanceFactory = Services.GetService<IInstanceFactory>();
            // default is transient
            return instanceFactory?.Make(InstanceType) ?? Activator.CreateInstance(InstanceType);
        }
    }
}
using System;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    public interface IServiceInstance : IDisposable
    {
        public Services Services { get; set; }
        public Type InstanceType { get; set; }
        public object Get()
        {
            var instanceFactory = Services.GetService<IInstanceFactory>();
            // default is transient
            return instanceFactory?.Make(InstanceType) ?? Activator.CreateInstance(InstanceType);
        }
    }
}
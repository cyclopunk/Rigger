using System;

namespace TheCommons.Forge.ManagedTypes.Lightweight.Defaults
{
    public class DefaultServiceInstance : IServiceInstance, IServiceAware
    {
        public void Dispose()
        {
            Services = null;
            InstanceType = null;
        }

        public Services Services { get; set; }
        public Type InstanceType { get; set; }
    }
}
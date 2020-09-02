using System;

namespace Rigger.Injection.Defaults
{
    public class DefaultServiceInstance : IServiceInstance, IServiceAware
    {
        public void Dispose()
        {
            Services = null;
            InstanceType = null;
        }

        public IServices Services { get; set; }
        public Type InstanceType { get; set; }
    }
}
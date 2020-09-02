using System;
using Rigger.ManagedTypes.Implementations;
using Rigger.Attributes;
using Rigger.Injection;

namespace Rigger.ManagedTypes.ComponentHandlers
{
    public class ConfigurationComponentHandler : IComponentHandler<ConfigurationAttribute>
    {
        public IServices Services { get; set; }
        public void HandleComponent(Type type)
        {
            /*
            var instance = new ManagedConstructorInvoker(container, type).Construct();

            if (instance != null)
            {
                container.Register(instance.GetType(), instance);
            }*/
            
        }
    }
}
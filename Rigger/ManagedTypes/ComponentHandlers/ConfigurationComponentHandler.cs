using System;
using TheCommons.Forge.ManagedTypes.Implementations;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.ComponentHandlers
{
    public class ConfigurationComponentHandler : IComponentHandler<ConfigurationAttribute>
    {
        public Services Services { get; set; }
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
using System;
using Rigger.ManagedTypes.Implementations;
using Rigger.ManagedTypes.Lightweight;
using Rigger.Attributes;

namespace Rigger.ManagedTypes.ComponentHandlers
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
using System;
using Rigger.ManagedTypes;
using Rigger.Attributes;
using Rigger.Injection;

namespace Rigger.ManagedTypes.ComponentHandlers
{
    public class BootstrapComponentHandler : IComponentHandler<BootstrapAttribute>
    {
        public IServices Services { get; set; }

        public void HandleComponent(Type type)
        {
            Services.Add(type, type, ServiceLifecycle.Singleton);

            Services.GetService(type); // make an instance

            //typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnCreateAttribute>(instance);
            //typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnStartupAttribute>(instance);
        }
    }
}
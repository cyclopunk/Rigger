using System;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.ComponentHandlers
{
    public class BootstrapComponentHandler : IComponentHandler<BootstrapAttribute>
    {
        public Services Services { get; set; }

        public void HandleComponent(Type type)
        {
            /*var typeRegistration = typeRegistry.Register(RegistrationType.Singleton, type);

            var instance = container.Get(type);

            typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnCreateAttribute>(instance);
            typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnStartupAttribute>(instance);*/
        }
    }
}
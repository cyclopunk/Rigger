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
            /*var typeRegistration = typeRegistry.Register(RegistrationType.Singleton, type);

            var instance = container.Get(type);

            typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnCreateAttribute>(instance);
            typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnStartupAttribute>(instance);*/
        }
    }
}
using System;
using System.Reflection;
using Rigger.Abstractions;
using Rigger.ManagedTypes;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.Injection;

namespace Rigger.ManagedTypes.ComponentHandlers
{
    public class BootstrapComponentHandler : IComponentHandler<BootstrapAttribute>
    {
        public IServices Services { get; set; }

        public void HandleComponent(Type type)
        {

            Services.Add(type, type, ServiceLifetime.Singleton);

            Services.GetService(type, CallSiteType.Method); // make an instance

            //typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnCreateAttribute>(instance);
            //typeRegistration.ImplementationType.Manager.InvokeAttributeMethods<OnStartupAttribute>(instance);
        }
    }
}
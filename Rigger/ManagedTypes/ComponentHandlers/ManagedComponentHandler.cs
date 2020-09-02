using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TheCommons.Core.Extensions;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.ComponentHandlers
{
    /// <summary>
    /// Default component handler for [Managed] classes. Adds a transient registration to the type registry.
    /// </summary>
    public class ManagedComponentHandler : IComponentHandler<ManagedAttribute>
    {
        public Services Services { get; set; }

        [Autowire] private ILogger _logger;

        public void HandleComponent(Type type)
        {
            ManagedAttribute attribute = type.GetCustomAttribute<ManagedAttribute>();
            if (attribute != null)
            {

                // the provided ServiceType on the attribute, the first interface or the concrete type is used (in that priority) as 
                // the lookup type.
                /*var lookupType = attribute.LookupType ?? type.GetInterfaces().FirstOr(type);

                var typeRegistration = _registry.Register(attribute.Scoped ? RegistrationType.Scoped : RegistrationType.Transient,
                                           lookupType, type);

                _logger.LogInformation($"FORGE: Registered {typeRegistration} managed type {type} as {lookupType}");*/
            }
        }
    }
}
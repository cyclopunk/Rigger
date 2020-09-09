using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Rigger.Extensions;
using Rigger.ManagedTypes;
using Rigger.Attributes;
using Rigger.Injection;

namespace Rigger.ManagedTypes.ComponentHandlers
{
    /// <summary>
    /// Default component handler for [Managed] classes. Adds a transient registration to the type registry.
    /// </summary>
    public class ManagedComponentHandler : IComponentHandler<ManagedAttribute>
    {
        public IServices Services { get; set; }

        [Autowire] private ILogger<ManagedComponentHandler> _logger;

        public void HandleComponent(Type type)
        {
            ManagedAttribute attribute = type.GetCustomAttribute<ManagedAttribute>();
            if (attribute == null) return;
            // the provided ServiceType on the attribute, the first interface or the concrete type is used (in that priority) as 
            // the lookup type.
            var lookupType = attribute.LookupType ?? type.GetInterfaces().FirstOr(type);

            if (type.HasTypeAttribute(typeof(ConditionAttribute)))
            {
                Services.AddConditionalService(lookupType, type,ServiceLifetime.Transient);
            }
            else
            {
                Services.Add(lookupType, type);
            }
        }
    }
}
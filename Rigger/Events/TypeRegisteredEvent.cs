using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Forge.ManagedTypes.ServiceLocator;

namespace TheCommons.Forge.Events
{
    public class TypeRegisteredEvent : ContainerEvent
    {
        public ServiceDescription Type { get; set; }


    }
}
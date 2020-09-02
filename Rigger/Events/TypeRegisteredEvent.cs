using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.ServiceLocator;

namespace Rigger.Events
{
    public class TypeRegisteredEvent : ContainerEvent
    {
        public ServiceDescription Type { get; set; }


    }
}
using Rigger.Injection;

namespace Rigger.Events
{
    public class TypeRegisteredEvent : ContainerEvent
    {
        public ServiceDescription Type { get; set; }


    }
}
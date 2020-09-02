using System;

namespace Rigger.Events
{
    public class IngotLoadedEvent : ContainerEvent
    {
        public Type IngotType { get; set; }
    }
}
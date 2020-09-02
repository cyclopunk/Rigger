using System;

namespace TheCommons.Forge.Events
{
    public class IngotLoadedEvent : ContainerEvent
    {
        public Type IngotType { get; set; }
    }
}
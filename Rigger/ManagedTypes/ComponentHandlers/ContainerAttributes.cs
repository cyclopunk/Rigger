using System;

namespace TheCommons.Forge.ManagedTypes.ComponentHandlers
{
    public class ContainerOptionsAttribute : Attribute
    {
        public bool Empty { get; set; } = false;
    }
}
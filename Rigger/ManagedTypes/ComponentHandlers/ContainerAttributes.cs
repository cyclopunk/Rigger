using System;

namespace Rigger.ManagedTypes.ComponentHandlers
{
    public class ContainerOptionsAttribute : Attribute
    {
        public bool Empty { get; set; } = false;
    }
}
using System;

namespace Rigger.Abstractions
{
    public class RequiresServiceAttribute : Attribute
    {
        public RequiresServiceAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get;  }

        
    }
}
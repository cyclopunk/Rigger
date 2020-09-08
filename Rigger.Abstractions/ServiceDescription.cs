using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rigger.Attributes;
using Rigger.ManagedTypes.Resolvers;

namespace Rigger.Injection
{
    public class ServiceDescription
    {
        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }
        public Func<IServices, object> Factory { get; set; }
        
        public ServiceLifetime Lifetime { get; set; }

        public bool IsConditional()
        {
            return ImplementationType.GetCustomAttribute(typeof(ConditionAttribute)) != null;
            
        }
    }
}
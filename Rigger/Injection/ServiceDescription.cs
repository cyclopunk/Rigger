using System;
using System.Collections.Generic;
using System.Linq;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.ManagedTypes.Resolvers;

namespace Rigger.Injection
{
    public class ServiceDescription
    {
        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }
        public Func<IServices, object> Factory { get; set; }
        
        public ServiceLifecycle LifeCycle { get; set; }

        public bool IsConditional()
        {
            return ImplementationType.HasTypeAttribute(typeof(ConditionAttribute));
        }
    }
}
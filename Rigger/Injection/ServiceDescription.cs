using System;
using System.Collections.Generic;
using System.Linq;

namespace Rigger.Injection
{
    public class ServiceDescription
    {
        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }
        public Func<IServices, object> Factory { get; set; }
        
        public ServiceLifecycle LifeCycle { get; set; }

        public List<Type> ExtraTypes = new List<Type>();
        public List<object> Singletons = new List<object>();

        public IEnumerable<Type> AllTypes()
        {
            return new List<Type> {ImplementationType}.Concat(ExtraTypes);
        }
    }
}
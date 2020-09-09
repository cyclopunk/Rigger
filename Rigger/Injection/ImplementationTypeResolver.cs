using System;
using System.Linq;

namespace Rigger.Injection
{
    /// <summary>
    /// Resovler that can create a instance based on the implementation type.
    /// </summary>
    public class ImplementationTypeResolver : IServiceResolver
    {
        public IServices Services { get; set; }

        internal ServiceDescription Description;

        internal Type ImplementationType;

        public ImplementationTypeResolver(IServices services, Type implementationType)
        {
            ImplementationType = implementationType;
            Services = services;
        }

        public virtual object Resolve()
        {
          
            var instanceFactory = Services.GetService<IInstanceFactory>();
            // default is transient
            return instanceFactory?.Make(ImplementationType) 
                              ?? Activator.CreateInstance(ImplementationType);
        }
    }
}
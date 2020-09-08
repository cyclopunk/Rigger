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

        internal object ResolvedService;

        public ImplementationTypeResolver(IServices services, Type implementationType)
        {
            this.ImplementationType = implementationType;
            this.Services = services;
            this.Description = Services.List().First(o => o.ImplementationType == this.ImplementationType);
        }

        public object Resolve()
        {
            if (ResolvedService != null && Description.Lifetime == ServiceLifetime.Singleton)
            {
                return ResolvedService;
            }

            var instanceFactory = Services.GetService<IInstanceFactory>();
            // default is transient
            ResolvedService = instanceFactory?.Make(Description.ImplementationType) 
                              ?? Activator.CreateInstance(Description.ImplementationType);

            return ResolvedService;
        }
    }
}
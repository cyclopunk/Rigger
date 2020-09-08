using System;
using System.Linq;

namespace Rigger.Injection
{
    /// <summary>
    /// Resovler that can create a instance based on the implementation type.
    /// </summary>
    public class OpenGenericResolver : IServiceResolver
    {
        public IServices Services { get; set; }

        internal ServiceDescription Description;
        
        internal Type OpenGenericType;
        internal Type LookupType;
        internal Type ImplementationType;

        internal object ResolvedService;

        public OpenGenericResolver(IServices services, Type lookupType)
        {
            
            OpenGenericType = lookupType.GetGenericTypeDefinition();
            LookupType = lookupType;
            Services = services;

            Description = Services.List().First(o => o.ServiceType == OpenGenericType);

            ImplementationType = Description.ImplementationType.MakeGenericType(LookupType.GenericTypeArguments);
        }

        public object Resolve()
        {
            if (ResolvedService != null && Description.Lifetime == ServiceLifetime.Singleton)
            {
                return ResolvedService;
            }

            var instanceFactory = Services.GetService<IInstanceFactory>(CallSiteType.ServiceProvider);
            // default is transient
            ResolvedService = instanceFactory?.Make(ImplementationType);

            return ResolvedService;
        }
    }
}
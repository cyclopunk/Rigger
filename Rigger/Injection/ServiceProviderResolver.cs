using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Rigger.Reflection;

namespace Rigger.Injection
{
    public class ServiceProviderResolver : DefaultResolver
    {
        public override object Resolve()
        {
            Description = Services.List(ServiceType).FirstOrDefault();

            if (Description == null)
            {
                return Description;
            }

            // these service types can be self-referencing, so we'll handle them with an Activator.
            IEnumerable<Type> serviceTypes = new List<Type>
            {
                typeof(IInstanceFactory),
                typeof(IServiceScopeFactory),
                typeof(IAutowirer),
                typeof(IConstructorActivator),
                typeof(IValueInjector),
            };

            if (serviceTypes.Contains(ServiceType))
            {
                var instance = Activator.CreateInstance(Description.ImplementationType);

                if (instance is IServiceAware factory)
                {
                    factory.Services = Services;
                }

                return instance;
            }

            // use the base resolve if this isn't the above types.

            return base.Resolve();
        }

        public ServiceProviderResolver(Type serviceType) : base(serviceType)
        {
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Rigger.Extensions;
using Rigger.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rigger.Core
{
    public class ServiceProviderBuilder
    {
        IServiceCollection services;
        public ServiceProviderBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceProvider NewRig()
        {
            Rig rig = new Rig();
            services.ForEach(o =>
            {
                var lifetime = o.Lifetime switch
                {
                    ServiceLifetime.Singleton => ServiceLifecycle.Singleton,
                    ServiceLifetime.Scoped => ServiceLifecycle.Scoped,
                    ServiceLifetime.Transient => ServiceLifecycle.Transient,
                    _ => ServiceLifecycle.Transient,
                };

                if (o.ImplementationInstance != null)
                {
                    rig.Register(o.ServiceType, o.ImplementationInstance);
                    return;
                }

                if (o.ImplementationFactory != null)
                {
                    // wrap impl factories
                    rig.Register(o.ServiceType, (services) =>
                    {
                        return o.ImplementationFactory.Invoke(services);
                    }, lifetime);

                    return;
                }

                rig.Register(o.ServiceType, o.ImplementationType, lifetime);
            });
            return rig;
        }
    }
}

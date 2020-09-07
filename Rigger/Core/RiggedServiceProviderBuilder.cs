using Microsoft.Extensions.DependencyInjection;
using Rigger.Extensions;
using Rigger.Injection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Rigger.Core
{
    public class RiggedServiceProviderBuilder
    {
        private readonly IServiceCollection services;
        public RiggedServiceProviderBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceProvider NewRig(string droneNamespace = "Drone.")
        {
            var rig = new Rig(droneNamespace);
            
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
                }
                else if (o.ImplementationFactory != null)
                {
                    // wrap impl factories
                    rig.Register(o.ServiceType, o.ImplementationFactory, lifetime);
                }
                else
                {
                    rig.Register(o.ServiceType, o.ImplementationType, lifetime);
                }
            });

            
            return rig;
        }
    }
}

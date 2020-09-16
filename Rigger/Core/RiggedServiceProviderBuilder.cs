using Microsoft.Extensions.DependencyInjection;
using Rigger.Extensions;
using Rigger.Injection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ServiceLifetime = Rigger.Injection.ServiceLifetime;

namespace Rigger.Core
{
    public class RiggedServiceProviderBuilder
    {
        private readonly IServiceCollection services;
        public RiggedServiceProviderBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceProvider NewRig(string[] droneNamespace)
        {
            var rig = new Rig(droneNamespace);
            
            services.ForEach(o =>
            {
                string typeName = o.ServiceType.ToString();

                var lifetime = o.Lifetime switch
                {
                    Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton => ServiceLifetime.Singleton,
                    Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped => ServiceLifetime.Scoped,
                    Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient => ServiceLifetime.Transient,
                    _ => ServiceLifetime.Transient,
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

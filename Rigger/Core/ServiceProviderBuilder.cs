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
    public class ServiceProviderBuilder
    {
        IServiceCollection services;
        public ServiceProviderBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceProvider NewRig()
        {
            Rig rig = new Rig("Drone");
           

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
                    Console.WriteLine($"Adding singleton {o.ServiceType} {o.ImplementationInstance}");
                    rig.Register(o.ServiceType, o.ImplementationInstance);
                }
                else if (o.ImplementationFactory != null)
                {
                    Console.WriteLine($"Adding implementation factory {o.ServiceType}");
                    // wrap impl factories
                    rig.Register(o.ServiceType, o.ImplementationFactory, lifetime);
                }
                else
                {

                    Console.WriteLine($"Registering {o.ServiceType.Name}, {o.ImplementationType.Name}, {lifetime}");
                    rig.Register(o.ServiceType, o.ImplementationType, lifetime);
                }
            });
            return rig;
        }
    }
}

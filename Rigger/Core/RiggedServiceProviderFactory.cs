using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rigger.Core
{
    public class RiggedServiceProviderFactory : IServiceProviderFactory<RiggedServiceProviderBuilder>
    {
        public string[] DroneNamespace { get; set; }

        public RiggedServiceProviderFactory(params string[] droneNamespace)
        {
            DroneNamespace = droneNamespace;
        }
        public RiggedServiceProviderBuilder CreateBuilder(IServiceCollection services)
        {
            return new RiggedServiceProviderBuilder(services);
        }

        public IServiceProvider CreateServiceProvider(RiggedServiceProviderBuilder containerBuilder)
        {
           return containerBuilder.NewRig(DroneNamespace);
        }
    }
}

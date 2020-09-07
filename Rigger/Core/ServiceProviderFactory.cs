using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rigger.Core
{
    class ServiceProviderFactory : IServiceProviderFactory<ServiceProviderBuilder>
    {
        public ServiceProviderBuilder CreateBuilder(IServiceCollection services)
        {
            return new ServiceProviderBuilder(services);
        }

        public IServiceProvider CreateServiceProvider(ServiceProviderBuilder containerBuilder)
        {
           return containerBuilder.NewRig();
        }
    }
}

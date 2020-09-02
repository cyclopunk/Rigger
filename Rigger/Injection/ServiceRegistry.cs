using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rigger.Injection
{
    public class ServiceRegistry : IServiceScopeFactory, IServiceProvider
    {
        public Services RegisteredServices { get; } = new Services();

        public IServiceScope CreateScope()
        {
            return new ServiceScope { Services = RegisteredServices.OfLifecycle(ServiceLifecycle.Scoped) };
        }

        public object GetService(Type serviceType)
        {
            return RegisteredServices.GetService(serviceType);
        }
    }

    // create instances with autowire constructors
}
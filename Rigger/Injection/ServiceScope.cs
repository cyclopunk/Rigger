using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rigger.Injection
{
    public class ServiceScopeFactory : IServiceScopeFactory, IServiceAware
    {
        public IServices Services { get; set; }
        public IServiceScope CreateScope()
        {
            Console.WriteLine("Created scope");
            return new ServiceScope().AddServices(Services);
        }
    }
    public class ServiceScope : IServiceProvider, IServiceScope, IServiceAware
    {
        private IServices services;
        private string id = Guid.NewGuid().ToString();

        internal ServiceScope()
        {

        }
        public IServiceProvider ServiceProvider => this;

        public IServices Services
        {
            get => services; 
            set => services = value.OfLifecycle(ServiceLifecycle.Singleton, ServiceLifecycle.Scoped);
        }

        public void Dispose()
        {
            services.DisposeScope();
        }

        public object GetService(Type serviceType)
        {
            if (typeof(IServiceProvider).IsAssignableFrom(serviceType))
            {
                return this;
            }

            var service = services.GetService(serviceType);

            Console.WriteLine($"Get scoped service {serviceType} : {service}");

            return service;
        }
    }
}
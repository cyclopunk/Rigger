using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rigger.Injection
{
    public class ServiceScopeFactory : IServiceScopeFactory, IServiceAware
    {
        public IServices Services { get; set; }
        public IServiceScope CreateScope()
        {
            return new ServiceScope().AddServices(Services);
        }
    }
    public class ServiceScope : IServiceProvider, IServiceScopeFactory, IServiceScope, IServiceAware
    {
        private IServices services;
        private IServiceProvider parent;

        private string id = Guid.NewGuid().ToString();

        internal ServiceScope()
        {

        }
        protected ServiceScope(ServiceScope parent)
        {
            this.parent = parent;
        }

        public IServiceProvider ServiceProvider => Services;

        public IServices Services
        {
            get => services; 
            set => services = value.OfLifecycle(ServiceLifecycle.Singleton, ServiceLifecycle.Scoped);
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this).AddServices(services);
        }

        public void Dispose()
        {
            services.DisposeScope();
        }

        public object GetService(Type serviceType)
        {

            var service = services.GetService(serviceType);

            return service;
        }
    }
}
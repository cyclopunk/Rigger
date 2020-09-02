using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rigger.ManagedTypes.Lightweight
{
    public class ServiceScope : IServiceProvider, IServiceScopeFactory, IServiceScope, IServiceAware
    {
        private Services services;
        private IServiceProvider parent;

        private string id = Guid.NewGuid().ToString();

        internal ServiceScope()
        {

        }
        protected ServiceScope(ServiceScope parent)
        {
            this.parent = parent;
        }

        public IServiceProvider ServiceProvider => parent;

        public Services Services
        {
            get => services; 
            set => services = value;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this);
        }

        public void Dispose()
        {
            services.Dispose();
        }

        public object GetService(Type serviceType)
        {

            var service = services.GetService(serviceType);
            
            if (service == null)
            {
                service = parent.GetService(serviceType);
                if (service != null)
                {
                    services.Add(serviceType, service.GetType());
                }
            }

            return service;
        }
    }
}
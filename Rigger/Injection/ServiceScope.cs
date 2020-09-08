using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FastDeepCloner;
using Microsoft.Extensions.DependencyInjection;
using Rigger.Extensions;

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
    public class ServiceScope : IServiceProvider, IServiceScope, IServiceAware,IServiceScopeFactory
    {
        internal ConcurrentDictionary<Type, object> ScopedServices = new ConcurrentDictionary<Type, object>();
        internal ServiceScope Parent;
        private string id = Guid.NewGuid().ToString();

        internal ServiceScope()
        {

        }

        public ServiceScope(ServiceScope parent)
        {
            this.Parent = parent;
        }
        public IServiceProvider ServiceProvider => this;

        public IServices Services { get; set; }

        public void Dispose()
        {
            ScopedServices.Values.OfType<IDisposable>().ForEach(o => o.Dispose());
            ScopedServices.Clear();
        }

        public object GetService(Type serviceType)
        {
            if (typeof(IServiceProvider).IsAssignableFrom(serviceType))
            {
                return this;
            }
            if (typeof(IServiceScopeFactory).IsAssignableFrom(serviceType))
            {
                return this;
            }

            if (Parent != null && Parent.ScopedServices.ContainsKey(serviceType))
            {
                return Parent.ScopedServices[serviceType];
            }

            var service = ScopedServices.GetOrAdd(serviceType, (k) => Services.GetService(k, CallSiteType.Scope));

            Console.WriteLine($"Get scoped service {serviceType} : {service}");

            return service;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this).AddServices(Services);
        }
    }
}
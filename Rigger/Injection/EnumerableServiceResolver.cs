using System;
using System.Collections.Generic;
using System.Linq;
using Rigger.Extensions;
using Rigger.Reflection;

namespace Rigger.Injection
{
    internal class EnumerableServiceResolver : IServiceResolver
    {
        public IServices Services { get; set; }

        internal Type ServiceType;

        internal object ResolvedService;
        internal IEnumerable<ServiceDescription> descriptions;
        internal Services.SingletonContainer singletons;

        public EnumerableServiceResolver(IServices services, 
            Type lookupType, 
            object enumerationSingletons, 
            IEnumerable<ServiceDescription> descriptions)
        {
            ServiceType = lookupType.GenericTypeArguments.FirstOrDefault();
            Services = services;

            if (enumerationSingletons is Services.SingletonContainer e)
            {
                this.singletons = e;
            }
            else if (enumerationSingletons != null)
            {
                this.singletons = new Services.SingletonContainer() {enumerationSingletons};
            }

            this.descriptions = descriptions;
        }

        public object Resolve()
        {
            if (ResolvedService != null)
            {
                return ResolvedService;
            }

            var type = typeof(List<>).MakeGenericType(ServiceType);

            var listActivator = new ExpressionActivator(type);

            var list = listActivator.Activate();

            var methodActivator = new SingleParameterMethodAccessor(type.GetMethod("Add"));
            
            // for transient / scoped services, look them up

            descriptions.Where(o => singletons == null || singletons.All(s => s.GetType() != o.ImplementationType)).ForEach(o => {
                if (o.Factory != null)
                {
                    methodActivator.Invoke(list, o.Factory(Services));
                }
                else
                {
                    methodActivator.Invoke(list, Services.GetService(o.ImplementationType, CallSiteType.Enumeration));
                }
            });

            if (singletons != null)
            {
                foreach (var n in singletons) {
                    methodActivator.Invoke(list, n);
                }
            }

            ResolvedService = list;

            return list;
        }
    }
}
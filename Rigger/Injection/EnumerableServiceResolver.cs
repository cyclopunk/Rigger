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

        public EnumerableServiceResolver(IServices services, Type lookupType)
        {
            ServiceType = lookupType.GenericTypeArguments.FirstOrDefault();
            Services = services;
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

            Services.List(ServiceType).ForEach(o => {
                if (o.Factory != null)
                {
                    methodActivator.Invoke(list, o.Factory(Services.GetService<IServiceProvider>() as IServices));
                }
                else
                {
                    methodActivator.Invoke(list, Services.GetService(o.ImplementationType, CallSiteType.Enumeration));
                }
            });

            ResolvedService = list;

            return list;
        }
    }
}
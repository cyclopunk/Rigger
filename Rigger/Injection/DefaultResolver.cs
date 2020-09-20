using System;
using System.Collections.Generic;
using System.Linq;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.ManagedTypes.Implementations;
using Rigger.Reflection;

namespace Rigger.Injection
{
    /// <summary>
    /// A resolver for 
    /// </summary>
    public class DefaultResolver : IServiceResolver
    {
        public IServices Services { get; set; }

        protected Type ServiceType;
        protected ServiceDescription Description;

        protected List<ZeroParameterMethodAccessor> Cache;

        protected IInstanceFactory InstanceFactory;

        public DefaultResolver(Type serviceType)
        {
            this.ServiceType = serviceType;
        }
        
        public virtual object Resolve()
        {
            Description ??= Services.List(ServiceType).FirstOrDefault();

            if (Description == null)
            {
                return null;
            }

            if (Description.Factory != null)
            {
                return Description.Factory(Services);
            }
            // we should have an instance factory by now.
            InstanceFactory ??= Services.GetService<IInstanceFactory>();
            // default is transient
            var instance = InstanceFactory?.Make(Description.ImplementationType);

            FireLifecycleMethods(instance);

            return instance;
        }

        protected void FireLifecycleMethods(object instance)
        {
            if (Cache == null)
            {
                Cache = new List<ZeroParameterMethodAccessor>();

                Description.ImplementationType
                    .MethodsWithAttribute(typeof(OnCreateAttribute)).ForEach(o =>
                    {
                        if (o.GetParameters().Any())
                        {
                            var invoker = new ManagedMethodInvoker(o).AddServices(Services);

                            invoker.Invoke(instance);

                            return;
                        }
                        var accessor = new ZeroParameterMethodAccessor(o);
                        
                        Cache.Add(accessor);

                        accessor.Invoke(instance);
                    });
            }
            else
            {
                Cache.ForEach(o => o.Invoke(instance));
            }
        }
    }
}
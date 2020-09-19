using System;
using Rigger.ManagedTypes;
using Rigger.Reflection;

namespace Rigger.Injection
{
    /// <summary>
    /// This instance factory will autowire all instances that it creates.
    /// </summary>
    public class AutowireInstanceFactory : IInstanceFactory, IServiceAware
    {
        public IServices Services { get; set; }

        private IConstructorActivator invoker;
        private IValueInjector injector;
        private IAutowirer autowirer;

        public object Make(Type type)
        {
            invoker ??= Services.GetService<IConstructorActivator>(CallSiteType.ServiceProvider);

            autowirer ??= Services.GetService<IAutowirer>(CallSiteType.ServiceProvider);

            injector ??= Services.GetService<IValueInjector>(CallSiteType.ServiceProvider);

            var instance = invoker?.Construct(type, new object[] {});

            if (instance == null)
            {
                return null;
            }

            autowirer?.Inject(instance);
            injector?.Inject(instance);

            if (instance is IServiceAware sa) sa.AddServices(Services);

            return instance;
        }
    }
}
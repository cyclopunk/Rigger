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
            object instance = null;

            if (typeof(IConstructorActivator).IsAssignableFrom (type))
            {
                instance = Activator.CreateInstance(type, new object[] { });
                if (instance is IServiceAware i) i.AddServices(Services);
                return instance;
            }

            invoker ??= Services.GetService<IConstructorActivator>();

            // loop protection
            if (typeof(IAutowirer).IsAssignableFrom (type))
            {
                instance = invoker?.Construct(type, new object[] { });
                if (instance is IServiceAware i) i.AddServices(Services);
                return instance;
            }
            
            // if we've created an instance above, return it and add the services if it is service aware.
            autowirer ??= Services.GetService<IAutowirer>();

            if (typeof(IValueInjector).IsAssignableFrom(type))
            {
                instance = invoker?.Construct(type, new object[] { });
                if (instance is IServiceAware i) i.AddServices(Services);
                return instance;
            }
            injector ??= Services.GetService<IValueInjector>();

            instance = invoker?.Construct(type, new object[] {});

            if (instance == null)
            {
                return null;
            }

            autowirer?.Inject(instance);
            injector?.Inject(instance);

            return instance;
        }
    }
}
using System;
using Rigger.ManagedTypes;
using Rigger.Reflection;

namespace Rigger.Injection
{
    public class AutowireInstanceFactory : IInstanceFactory, IServiceAware
    {
        public IServices Services { get; set; }

        public object Make(Type type)
        {
            IConstructorActivator invoker = Services.GetService<IConstructorActivator>();
            IAutowirer autowire = Services.GetService<IAutowirer>();

            var instance = invoker?.Construct(type, new object[] {});

            if (instance is IServiceAware i)
            {
                i.Services = Services;
            }

            autowire?.Inject(instance);

            return instance;
        }
    }
}
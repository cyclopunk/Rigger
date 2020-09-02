using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using TheCommons.Forge.ManagedTypes.Implementations;
using TheCommons.Forge.ManagedTypes.Resolvers;
using TheCommons.Forge.ManagedTypes.ServiceLocator;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.Implmentors
{
    public class SingletonImplementor : FactoryImplementor
    {
        private IContainer _container;
        private TypeManager _manager;
        private ManagedConstructorInvoker invoker;
        private object instance;

        public SingletonImplementor(IContainer container, Func<Type> typeFactory) : base(typeFactory)
        {
            this._container = container;
        }

        public override object GetInstance(params object[] implementorParams)
        {
            if (instance != null)
            {
                return instance;
            }

            var type = TypeFactory();
            
            _manager = new TypeManager(_container, type);

            invoker = new ManagedConstructorInvoker(_container, type);

            instance = invoker.Construct(implementorParams);

            _manager.Autowire(instance);
            
            _manager.InvokeAttributeMethods<OnCreateAttribute>(instance);

            var resolver = _container.Get<IValueResolver>();

            resolver?.Resolve(instance);

            return instance;
        }
    }
}
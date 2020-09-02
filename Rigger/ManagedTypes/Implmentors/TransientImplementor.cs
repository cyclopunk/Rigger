using System;
using TheCommons.Forge.ManagedTypes.Implementations;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Resolvers;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.ServiceLocator
{
    public class TransientImplementor : FactoryImplementor
    {
        private IContainer _container;
        private TypeManager _manager;
        private ManagedConstructorInvoker invoker;
        public TransientImplementor(IContainer container, Func<Type> typeFactory) : base(typeFactory)
        {
            this._container = container;
        }

        public override object GetInstance(params object[] implementorParams)
        {
            
            var type = TypeFactory();
            _manager ??= new TypeManager(_container, type);
            invoker ??= new ManagedConstructorInvoker(_container, type);

            var instance = invoker.Construct(implementorParams);

            _manager.Autowire(instance);
            
            _manager.InvokeAttributeMethods<OnCreateAttribute>(instance);
            
            var resolver = _container.Get<IValueResolver>();

            resolver?.Resolve(instance);

            return instance;
        }
    }
}
using System;
using Rigger.ManagedTypes.Implementations;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.Resolvers;
using Rigger.Attributes;

namespace Rigger.ManagedTypes.ServiceLocator
{
    public class TransientImplementor : FactoryImplementor
    {
        private IContainer _container;
        private ManagedConstructorInvoker invoker;
        public TransientImplementor(IContainer container, Func<Type> typeFactory) : base(typeFactory)
        {
            this._container = container;
        }

        public override object GetInstance(params object[] implementorParams)
        {
            /*

            var type = TypeFactory();
            _manager ??= new TypeManager(_container, type);
            invoker ??= new ManagedConstructorInvoker(_container, type);

            var instance = invoker.Construct(implementorParams);

            _manager.Autowire(instance);
            
            _manager.InvokeAttributeMethods<OnCreateAttribute>(instance);
            
            var resolver = _container.Get<IValueResolver>();

            resolver?.Resolve(instance);

            return instance;*/
            return null;
        }
    }
}
using System;
using System.Threading;
using TheCommons.Forge.ManagedTypes.Implementations;

namespace TheCommons.Forge.ManagedTypes.ServiceLocator
{
    /// <summary>
    /// Implementor that uses a ThreadLocal
    /// </summary>
    public class LocalThreadImplementor: FactoryImplementor
    {
        private IContainer _container;
        private volatile ManagedConstructorInvoker invoker;
        private ThreadLocal<object> threadLocalFactory;
        public LocalThreadImplementor(IContainer container, Func<Type> typeFactory) : base(typeFactory)
        {
            this._container = container;
            threadLocalFactory = new ThreadLocal<object>(() => invoker.Construct());
        }

        public override object GetInstance(params object[] implementorParams)
        {
            invoker ??= new ManagedConstructorInvoker(_container, TypeFactory());

            return threadLocalFactory.Value;
        }
    }
}
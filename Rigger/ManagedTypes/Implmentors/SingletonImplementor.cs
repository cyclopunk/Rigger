using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using Rigger.ManagedTypes.Implementations;
using Rigger.ManagedTypes.Resolvers;
using Rigger.ManagedTypes.ServiceLocator;
using Rigger.Attributes;

namespace Rigger.ManagedTypes.Implmentors
{
    public class SingletonImplementor : FactoryImplementor
    {
        private IContainer _container;
        private ManagedConstructorInvoker invoker;
        private object instance;

        public SingletonImplementor(IContainer container, Func<Type> typeFactory) : base(typeFactory)
        {
            this._container = container;
        }

        public override object GetInstance(params object[] implementorParams)
        {
            return instance;
        }
    }
}
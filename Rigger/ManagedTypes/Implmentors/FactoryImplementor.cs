using System;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.Implmentors;

namespace Rigger.ManagedTypes.ServiceLocator
{
    public abstract class FactoryImplementor : IImplementor
    {
        public Func<Type> TypeFactory { get; set; }

        protected FactoryImplementor(Func<Type> typeFactory)
        {
            TypeFactory = typeFactory;
        }

        public abstract object GetInstance(params object[] implementorParams);
    }
}
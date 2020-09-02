using System;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Implmentors;

namespace TheCommons.Forge.ManagedTypes.ServiceLocator
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
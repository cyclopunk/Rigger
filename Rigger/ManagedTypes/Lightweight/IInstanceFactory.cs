using System;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    public interface IInstanceFactory
    {
        public object Make(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
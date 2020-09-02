using System;

namespace TheCommons.Forge.ManagedTypes
{
    public interface IConstructorInvoker<T>
    {
        T Construct(Type t, params object[] parameters);
    }
    public interface IConstructorInvoker
    {
   //     object Construct(params object[] parameters);
        object Construct(Type type, params object[] parameters);
    }
}
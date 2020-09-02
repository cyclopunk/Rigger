using System;

namespace Rigger.Reflection
{
    public interface IConstructorInvoker<out T>
    {
        T Construct(params object[] parameters);
    }
    public interface IConstructorActivator
    {
        object Construct(params object[] parameters);
    }
}
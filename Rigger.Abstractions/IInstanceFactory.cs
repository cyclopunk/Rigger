using System;

namespace Rigger.Injection
{
    public interface IInstanceFactory
    {
        public object Make(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
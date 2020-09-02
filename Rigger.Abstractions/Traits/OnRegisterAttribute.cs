using System;

namespace Rigger.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class OnRegisterAttribute : Attribute, ILifecycle
    {
        
    }
}
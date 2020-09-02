using System;

namespace Rigger.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class PreRegistrationAttribute : Attribute, ILifecycle
    {
        
    }
}
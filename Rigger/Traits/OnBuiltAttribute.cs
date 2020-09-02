using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Lifecycle attribute that will fire when the application is successfully built.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class OnBuiltAttribute : Attribute, ILifecycle
    {
        
    }
}
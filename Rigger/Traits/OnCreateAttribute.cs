using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// OnCreate will allow a method to be run at the Create lifecycle hook. This occurs after the
    /// OnStartup lifecycle hook.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OnCreateAttribute : Attribute, ILifecycle
    {

    }
}
using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// OnDestroy will allow a method to be run at the Destroy/Dispose lifecycle hook. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OnDestroyAttribute : Attribute, ILifecycle
    {

    }
}
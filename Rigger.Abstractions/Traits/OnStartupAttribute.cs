using System;

namespace Rigger.Attributes
{
    
    /// <summary>
    /// OnStartup marks a method that will be run on the startup of the application. This occurs after OnCreate
    /// if the class is a bootstrap class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OnStartupAttribute : Attribute, ILifecycle
    {
        
    }
}
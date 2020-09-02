using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Mark a class as a Module. Module's are used to augment the basic functionality of
    /// the Forge application context. Any class that is marked as a module
    /// needs a constructor with ApplicationContext as a parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
        /// <summary>
        /// Define the priority of a module, if multiple modules are defined, they will be ran in ascending order
        /// (higher priority modules are ran last). The Default Forge module (with basic configuration) has a priority of -1.
        /// </summary>
        public int Priority { get; set; }
    }
}
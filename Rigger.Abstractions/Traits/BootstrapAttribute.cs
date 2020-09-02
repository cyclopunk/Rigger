using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Attribute to mark a class as a bootstrap class. Bootstrap classes will be managed
    /// and will have their OnStartup methods triggered at the proper lifecycle hook.
    /// Bootstrap types can depend on one another.
    ///
    /// Bootstrap classes will not be available for autowiring and are only
    /// meant to run code during the startup lifecycle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BootstrapAttribute : Attribute
    {
        /// <summary>
        /// This property will mark another class as a dependency and that class instance
        /// will be loaded before this class
        /// </summary>
        public Type DependsOn { get; set; }

    }
}

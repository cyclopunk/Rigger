using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Attribute that will mark a class as a singleton. One instance this type
    /// will exist in an Application container at a time. Singleton services will run
    /// the onStartup, onCreate and OnDestroy lifecycle hooks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SingletonAttribute : Attribute
    {
        /// <summary>
        /// The type that is used to lookup this service. By default the first interface is used
        /// but this setting can override that. Note, the underlying type MUST implement this type.
        /// </summary>
        public Type LookupType { get; set; }

        public SingletonAttribute()
        {

        }

        public SingletonAttribute(Type lookupType)
        {
            this.LookupType = lookupType;
        }
    }
}

using System;

namespace Rigger.Attributes
{

    /// <summary>
    /// Attribute to mark an instance as a managed type. All managed types will
    /// be added managed by the DI framework and will be available to other services / components
    /// at runtime via the Autowire attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ManagedAttribute : Attribute
    {
        /// <summary>
        /// The type that is used to lookup this service. By default the first interface is used
        /// but this setting can override that. Note, the underlying type MUST implement this type.
        /// </summary>
        public Type LookupType { get; set; }
        public bool Scoped { get; set; }

        public ManagedAttribute()
        {

        }

        public ManagedAttribute(Type lookupType, bool scoped = false)
        {
            this.LookupType = lookupType;
            this.Scoped = scoped;
        }
    }
}
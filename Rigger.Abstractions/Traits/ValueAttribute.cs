using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// This attribute will tell the container that a parameter, field or property should have its value
    /// automatically injected. The IValueResolver class is responsible for handling value injection. Currently only
    /// string injection is possible and the default value injector uses the IConfigurationService. If the Key is not defined
    /// it will use the Member name to lookup the value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class ValueAttribute : Attribute
    {
        /// <summary>
        /// The configuration key to look up the value.
        /// </summary>
        public string Key { get; set; }
    }
}
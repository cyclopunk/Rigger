using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Specifies a condition on which a service, field or property should be loaded. This functionality allows for
    /// configuration to determine which underlying implementation is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class ConditionAttribute : Attribute
    {
        /// <summary>
        /// A string expression that is used to determine whether this condition is true or false.
        /// The parameter "config" can be used to interface with the IConfigurationService
        /// (e.g. config.Get("somevalue") == "someothervalue").
        /// </summary>
        public string Expression { get; set; }
    }
}
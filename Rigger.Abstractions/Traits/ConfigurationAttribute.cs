using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Marks a class as a Forge configuration class. Configuration classes are instantiated and
    /// their constructor is injected with all objects available in the application context (IContainer, IConfigurationService, etc)
    /// as well as Values from the IValueResolver if one is available.
    ///
    /// Configurations are instantiated in order of priority, configurations with a lower priority are
    /// instantiated first. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ConfigurationAttribute : Attribute
    {
        public int Priority { get; set; }
    }
}
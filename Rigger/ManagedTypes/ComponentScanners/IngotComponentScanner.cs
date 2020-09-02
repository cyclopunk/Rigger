using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheCommons.Core.Extensions;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.ComponentScanners
{
    public class IngotAttribute : Attribute
    {

    }
    public class IngotConfiguration
    {
        public string Name { get; set; }
        public List<Type> types { get; set; }
    }
    /// <summary>
    /// Ingots are self contained plugins within types. They might contain a module, a configuration
    /// and two singletons. 
    /// </summary>
    public class IngotComponentScanner : AttributeBasedComponentScanner<IngotAttribute>
    {
        public Services Services { get; set; }

    }
}
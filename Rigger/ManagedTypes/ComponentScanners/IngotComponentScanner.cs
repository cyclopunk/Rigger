using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rigger.Extensions;
using Rigger.Attributes;
using Rigger.Injection;

namespace Rigger.ManagedTypes.ComponentScanners
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
        public IServices Services { get; set; }

    }
}
#pragma warning disable 649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rigger.Extensions;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes;

namespace Rigger.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Search for managed components. This should be ran last as they may depend on singletons and
    /// other autowire types.
    /// </summary>
    public class ManagedComponentScanner : IComponentScanner
    {
        public IServices Services { get; set; }
        [Autowire] private IComponentHandler<ManagedAttribute> Handler;
        
        public IEnumerable<Type> ComponentScan(params Assembly[] assemblies)
        {
            var managedComponents = assemblies
                .Select(o => o.TypesWithAttribute<ManagedAttribute>())
                .Combine()
                .Where(w => !w.IsNested).ToList(); // No nested types, these are ingots and need to be loaded manually
            
            managedComponents
                .ForEach(Handler.HandleComponent);

            return managedComponents;
        }
    }
}
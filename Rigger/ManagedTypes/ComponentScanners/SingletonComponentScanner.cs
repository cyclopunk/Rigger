using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TheCommons.Core.Extensions;
using TheCommons.Forge.ManagedTypes.ComponentHandlers;
using TheCommons.Traits.Attributes;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Lightweight;

namespace TheCommons.Forge.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Component scanner that will scan an assembly for singletons.
    /// </summary>
    public class SingletonComponentScanner : IComponentScanner<IEnumerable<Type>>
    {
        public Services Services { get; set; }

        [Autowire] private IContainer _container;
        [Autowire] private ILogger _logger;
        [Autowire] private IComponentHandler<SingletonAttribute> Handler { get; set; }
        public IEnumerable<Type> ComponentScan(params Assembly[] assemblies)
        {
            var managedComponents = assemblies.Select(o => o.TypesWithAttribute<SingletonAttribute>())
                .Combine()
                .Where(w => !w.IsNested).ToList(); // No nested types, these are ingots and need to be loaded manually
            
            managedComponents.ForEach(Handler.HandleComponent);

            return managedComponents;
        }
    }
}
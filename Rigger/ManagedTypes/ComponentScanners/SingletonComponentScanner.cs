using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Rigger.Extensions;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes;

namespace Rigger.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Component scanner that will scan an assembly for singletons.
    /// </summary>
    public class SingletonComponentScanner : IComponentScanner<IEnumerable<Type>>
    {
        public IServices Services { get; set; }

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
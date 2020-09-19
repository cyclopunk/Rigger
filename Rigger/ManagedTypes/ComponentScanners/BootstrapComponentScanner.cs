using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rigger.Extensions;
using Rigger.Dependencies;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes;

namespace Rigger.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Component Scanner that provides the default handling of classes with the Bootstrap class
    /// attribute.
    /// </summary>
    public class BootstrapComponentScanner : IComponentScanner
    {
        public IServices Services { get; set; }
        [Autowire] private IComponentHandler<BootstrapAttribute> Handler { get; set; }

        public IEnumerable<Type> ComponentScan(params Assembly[] assemblies)
        {
            DependencyTree bootstrapDependencyTree  = new DependencyTree();

            assemblies.Select(o => o.TypesWithAttribute<BootstrapAttribute>())
                .Combine()
                .Where(w => !w.IsNested) // No nested types, these are ingots and need to be loaded manually
                .ForEach(o =>
                {
                    bootstrapDependencyTree.AddDependency(o);
                });

            var bootstrapComponents = bootstrapDependencyTree.DepthFirst()
                .Select(n => n.Type)
                .Where(o => o != null);

            bootstrapComponents.ForEach(Handler.HandleComponent);

            return bootstrapComponents;
        }

    }
}
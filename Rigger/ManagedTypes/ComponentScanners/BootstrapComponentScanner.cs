﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheCommons.Core.Extensions;
using TheCommons.Forge.Dependencies;
using TheCommons.Forge.ManagedTypes.ComponentHandlers;
using TheCommons.Traits.Attributes;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Lightweight;

namespace TheCommons.Forge.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Component Scanner that provides the default handling of classes with the Bootstrap class
    /// attribute.
    /// </summary>
    public class BootstrapComponentScanner : IComponentScanner<IEnumerable<Type>>
    {
        public Services Services { get; set; }
        [Autowire] private IContainer Container { get; set; }
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
                .Map(n => n.Type)
                .FindAll(o => o != null);

            bootstrapComponents.ForEach(Handler.HandleComponent);

            return bootstrapComponents;
        }

    }
}
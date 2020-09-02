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
    /// Component scanner that will handle bootstrap classes (Lifecyces, Bootstraps, Configuration)
    /// </summary>
    public class RootConfigurationComponentScanner : IComponentScanner<IEnumerable<Type>>
    {
        public Services Services { get; set; }
        [Autowire] private IContainer Container { get; set; }
        [Autowire] private IComponentHandler<ConfigurationAttribute> ConfigurationHandler { get; set; }


        public DependencyTree BootstrapDependencyTree { get; set; } = new DependencyTree();
        public IEnumerable<Type> ComponentScan(params Assembly[] assemblies)
        {
            /*
            assemblies
                .Select(a => a.TypesWithInterface<ILifecycle>())
                .Combine()
                .ForEach(o =>
                {
                    Container.Context.TypeRegistry.Register(RegistrationType.Ephemeral,
                        o, o);
                });


            assemblies
                .Select(a =>  a.TypesWithAttribute<ConfigurationAttribute>() )
                .Combine()
                .Where(w => !w.IsNested) // No nested types, these are ingots and need to be loaded manually
                .OrderByDescending(x => x.GetCustomAttribute<ConfigurationAttribute>().Priority)
                .ForEach(ConfigurationHandler.HandleComponent);

            */
            return null;
        }
    }
}
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
    /// Component scanner that will handle bootstrap classes (Lifecyces, Bootstraps, Configuration)
    /// </summary>
    public class RootConfigurationComponentScanner : IComponentScanner<IEnumerable<Type>>
    {
        public IServices Services { get; set; }
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
using System;
using System.Linq;
using System.Reflection;
using TheCommons.Core.Extensions;
using TheCommons.Forge.ManagedTypes.ComponentScanners;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.ComponentHandlers
{
    /// <summary>
    /// Component Handler for Ingots. Ingots are classes that contain
    /// Managed Types as nested classes. This will allow applications to be built without polluting the entire
    /// assembly. Ingots will only be loaded if LoadIngot(Type) is called or the
    /// Ingot assembly scanner is used directly. That scanner is not loaded as a default
    /// ApplicationContext scanners.
    /// </summary>
    public class IngotComponentHandler : IComponentHandler<IngotAttribute>
    {
        public Services Services { get; set; }
        [Autowire]
        private IComponentHandler<BootstrapAttribute> BootstrapComponentHandler { get; set; }
        [Autowire]
        private IComponentHandler<ConfigurationAttribute> ConfigurationComponentHandler { get; set; }

        [Autowire]
        private IComponentHandler<ManagedAttribute> ManageComponentHandler { get; set; }

        [Autowire]
        private IComponentHandler<SingletonAttribute> SingletonComponentHandler { get; set; }


        public void HandleComponent(Type type)
        {
            Type[] nestedTypes = type.GetNestedTypes(ReflectionExtensions.defaultBindingFlags);

            // Register managed types in the defined order.

            nestedTypes
                .Where( o=> o.GetCustomAttribute<ConfigurationAttribute>() != null)
                .ForEach(ConfigurationComponentHandler.HandleComponent);
            
            nestedTypes
                .Where( o=> o.GetCustomAttribute<ManagedAttribute>() != null)
                .ForEach(ManageComponentHandler.HandleComponent);
            
            nestedTypes
                .Where( o=> o.GetCustomAttribute<SingletonAttribute>() != null)
                .ForEach(SingletonComponentHandler.HandleComponent);

            nestedTypes
                .Where( o=> o.GetCustomAttribute<BootstrapAttribute>() != null)
                .ForEach(BootstrapComponentHandler.HandleComponent);
        }
    }
}
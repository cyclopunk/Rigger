using System;
using Microsoft.Extensions.Logging;
using TheCommons.Core.Configuration;
using TheCommons.Core.Configuration.Sources;
using TheCommons.Core.ValueConverters;
using TheCommons.Forge.ManagedTypes.ComponentHandlers;
using TheCommons.Forge.ManagedTypes.ComponentScanners;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Forge.ManagedTypes.Resolvers;
using TheCommons.Logging;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.Features
{
    /// <summary>
    /// THis module provides the default behavior for Crucible. It contains the basic component scanners
    /// for all ManagedType attributes as well as default implementations of the instance factory, type registries
    /// and configuration service.
    ///
    /// By default, a console logger is installed.
    ///
    /// These parameters can be overriden by modules in other assemblies.
    /// </summary>
    [Module(Priority = -1)]
    public class DefaultModule : IServiceAware
    {
        public Services Services { get; set; }
        public DefaultModule(Services services)
        {
            services.Add(typeof(ILogger<>), typeof(ConsoleLogger<>));
            /*
            ctx.EventRegistry = new EventRegistry();
            ctx.ConfigurationService = new DefaultConfigurationService().AddSource(new MapConfigurationSource());
            ctx.TypeRegistry = new ManagedTypeRegistry();
            ctx.Logger = new ConsoleLogger().Logger;
            ctx.ValueResolver = new ConfigurationValueResolver();

            ctx.InjectContext(ctx.Container, ctx.EventRegistry, ctx.TypeRegistry, ctx.ValueResolver, ctx.ConfigurationService);

            ctx.TypeRegistry.Register<IComponentHandler<ConfigurationAttribute>>(typeof(ConfigurationComponentHandler));
            ctx.TypeRegistry.Register<IComponentHandler<ManagedAttribute>>(typeof(ManagedComponentHandler));
            ctx.TypeRegistry.Register<IComponentHandler<SingletonAttribute>>(typeof(SingletonComponentHandler));
            ctx.TypeRegistry.Register<IComponentHandler<BootstrapAttribute>>(typeof(BootstrapComponentHandler));
            ctx.TypeRegistry.Register<IComponentHandler<IngotAttribute>>(typeof(IngotComponentHandler));

            ctx.TypeRegistry.Register<IValueConverter<string, bool>>(typeof(StringToBoolConverter));

            // default component handlers.

            // add the default component scanners.

            ctx.ComponentScanners.AddRange(new Type[]{
                typeof(RootConfigurationComponentScanner),
                typeof(ManagedComponentScanner),
                typeof(SingletonComponentScanner),
                typeof(BootstrapComponentScanner)
            });

            ctx.TypeRegistry.Register<TypeManager, TypeManager>();
            ctx.TypeRegistry.Register(RegistrationType.Transient, typeof(ILogger<>), typeof(Logger<>));*/

        }

    }
}
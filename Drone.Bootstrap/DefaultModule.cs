using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rigger;
using Rigger.Attributes;
using Rigger.Implementations;
using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.ManagedTypes.ComponentScanners;
using Rigger.ManagedTypes.Features;
using Rigger.ManagedTypes.Implementations;
using Rigger.Reflection;

namespace Drone.Bootstrap
{
    /// <summary>
    /// Module that sets up a default setup for a rigged application.
    ///
    /// This default can be changed by other modules in the component scanning paths.
    /// </summary>
    [Module(Priority = -1)]
    public class DefaultModule
    {
        public DefaultModule(IServices services)
        {
            services
                .Add(typeof(ILoggerFactory), typeof(ConsoleLogger))
                .Add<IAutowirer, ContainerAutowirer>() // add autowiring
                .Add<IConstructorActivator,ManagedConstructorInvoker>() // faster activator
                .Add<IInstanceFactory,AutowireInstanceFactory>() // factory that will create autowired instances
                .Add<IComponentHandler<SingletonAttribute>, SingletonComponentHandler>()  // add component handlers for managed types
                .Add<IComponentHandler<ManagedAttribute>, ManagedComponentHandler>() 
                .Add<IComponentHandler<BootstrapAttribute>, BootstrapComponentHandler>()
                .Add<IComponentScanner, SingletonComponentScanner>() 
                .Add<IComponentScanner, ManagedComponentScanner>()
                .Add<IComponentScanner, BootstrapComponentScanner>()
                .Add(typeof(ILogger<>), typeof(Logger<>)) // logging
                .Add<IServiceProvider>(services) // reference to self
                .Add<IServiceScopeFactory, ServiceScopeFactory>(); // scoped support


        }
    }
}

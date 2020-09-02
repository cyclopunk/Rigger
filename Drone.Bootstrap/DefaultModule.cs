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
    /// Module that sets up a default environment for Rigger.
    /// </summary>
    [Module]
    public class DefaultModule
    {
        public DefaultModule(Services services)
        {
            services.Add(typeof(ILoggerFactory), typeof(ConsoleLogger))
                .Add<IAutowirer>(new ContainerAutowirer()) // add autowiring
                .Add<IComponentHandler<SingletonAttribute>, SingletonComponentHandler>() 
                .Add<IComponentHandler<ManagedAttribute>, ManagedComponentHandler>()
                .Add<IComponentScanner, SingletonComponentScanner>() 
                .Add<IComponentScanner, ManagedComponentScanner>()
                .Add(typeof(ILogger<>), typeof(Logger<>))
                .Add<IConstructorActivator>(new ManagedConstructorInvoker())
                .Add<IInstanceFactory>(new AutowireInstanceFactory())
                .Add<IServiceProvider>(services)
                .Add<IServiceScopeFactory>(services);


        }
    }
}

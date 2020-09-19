using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rigger;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.ManagedTypes.ComponentScanners;
using Rigger.ManagedTypes.Implementations;
using Rigger.Injection.Defaults;
using Rigger.Reflection;
using Rigger.ValueConverters;

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
                .Add<ExpressionTypeResolver, ExpressionTypeResolver>() // conditional services support.
                .Add<IComponentHandler<SingletonAttribute>, SingletonComponentHandler>()  // add component handlers for managed types
                .Add<IComponentHandler<ManagedAttribute>, ManagedComponentHandler>() 
                .Add<IComponentHandler<BootstrapAttribute>, BootstrapComponentHandler>()
                .Add<IComponentScanner, SingletonComponentScanner>() 
                .Add<IComponentScanner, ManagedComponentScanner>()
                .Add<IComponentScanner, BootstrapComponentScanner>()
                .Add<IValueConverter<string,bool>, StringToBoolConverter>()
                .Add<IValueConverter<string, int>, StringToIntConverter>()
                .Add<IValueConverter<string, double>, StringToDoubleConverter>()
                .Add<IValueConverter<string, long>, StringToLongConverter>()
                .Add<IValueConverter<string, object>, JsonValueConverter<object>>()
                .Add(typeof(ILogger<>), typeof(Logger<>)) // logging
                .Add<IServiceScopeFactory, ServiceScopeFactory>(); // scoped support


        }
    }
}

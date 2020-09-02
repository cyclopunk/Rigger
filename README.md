# The Forge

Forge is a framework for dependency injection, inversion of control and container-managed instances. It is a part of TheCommons project. It is being developed to address some feature gaps in common C# DI frameworks. Some may notice that it is very Spring Boot-esq and that is intentional. It is the hope that this platform can be used to quickly build projects with advanced features. 

Forge is also designed to work well with other TheCommons libraries such as Cache, Logger and Mapper in order to provide those systems
automatically to any application developed with Forge. This will allow any Forge Application to receive enhancements as those
projects are being worked on.

Forge is designed to be fast (not as fast as "new", but not as slow as Activtor), lightweight (simple interfaces), extensible (Plugins / Modules) and easy to use (configuration-lite). 

### Features
+ Attribute based registration and autowiring.
+ Configuration-based Value Injection
+ Transient, Singleton, Scopped and Thread based registrations
+ Conditional Service Registration
+ Built-in services for container-wide use (Configuration, Logging, Eventing)
+ Instance lifecycle hooks (OnCreate, OnStartup, OnBuilt, OnDestroy)
+ Application Event Pipeline
+ Cached expression-based Member Invokers and Constructor Activators for speedy autowiring.
+ Module / Plugin System for Extensibility (Ingots)
+ Fail Fast application startup.
+ Managed startup lifecycle and dependency ordering (Module -> Configuration -> Managed -> Singleton -> Bootstrap).
+ Include Additional Forge libraries for added functionality :
  - Forge.Cloud, unified cloud services automatically configured and injected into the container
  - Forge.API, automatically bootstraps a GraphQL endpoint that integrates your entity model
  - Forge.Data, Wide Column Storage, ML Services, And Statistical Services
  - Forge.Chronos, Timer functionality for services.

### TODO 
- Simplified Registration
- Improved Container Validation
- Better Lifecycle Management
- Refactor InstanceRegistry / TypeRegistry / ConcreteTypes to be more simple
- Open generic type registration (also Refactor generic type handling in most code)

## Quickstart

In order to create a application, it is as simple as inheriting from the ApplicationContainer class. Instantiating that class will trigger
the configuration of the application. This will start the component scanning part of the framework and it will discover all components
that are marked with a Managed Type attribute. You can pass in additional assemblies to the ApplicationContainer constructor in order to 
include other Assemblies in your application. This will allow a plugin type style of development.

```
class YourApplication : ApplicationContainer
    {
        static void Main(string[] ar
        {
            YourApplication ex = new YourApplication();
        }
    }
```

Alternatively, if you are implementing a .Net Core application you can register forge as your service provider like so:

```
public class Startup { 
  public IServiceProvider ConfigureServices(IServiceCollection services) {
     var provider = new ForgeServiceProvider();
    
     provider.RegisterAll(services);
  
     return provider;
  }
}
```

## Managed Type Attributes

There are five types of attributes that will mark a class as a *Managed Type* and they are instantiated and registered at different times 
at the application lifecycle. The five types are Module, Configuration, Singleton, Managed, Bootstrap and each are treated a bit differently
by the container. The following section will explain all of these Managed Type Attributes and give an example on how they work.

### Modules

Modules are classes that modify the basic functionality of the framework. They are simply classes with a constructor that take the ApplicationContext as an argument. In this constructor
the implementor can change the basic building blocks of the framework. The default module that gets loaded is below.

Modules are instantiated before any registration or scanning is done.

```
[Module]
public class DefaultModule
{
    public DefaultModule(ApplicationContext ctx)
    {
        ctx.EventRegistry = new EventRegistry();
        ctx.ConfigurationService = new DefaultConfigurationService().AddSource(new MapConfigurationSource());
        ctx.InstanceRegistry = new InstanceRegistry();
        ctx.TypeRegistry = new ManagedTypeRegistry();
        ctx.InstanceFactory = new DefaultInstanceFactory();
        ctx.Logger = new ConsoleLogger().Logger;
        ctx.ComponentScanners = new IComponentScanner<IEnumerable<Type>>[]
        {
            new RootConfigurationComponentScanner(),
            new ManagedComponentScanner(),
            new SingletonComponentScanner(),
            new BootstrapComponentScanner()
        };

        ctx.Autowire(ctx.ConfigurationService, ctx.InstanceRegistry, ctx.TypeRegistry, ctx.InstanceFactory, ctx.Logger, ctx.EventRegistry);
        ctx.Autowire(ctx.ComponentScanners);
    }

}
```

### Configuration

Configurations are loaded after Modules in order of priority (highest first). The constructor
can be injected with components that have been registered and values that
are loaded from the container configuration service. 

There are some caveats to configurations.

- Configurations cannot use field or property autowiring. 
- Configurations are instantiated before Managed / Singleton services, so you cannot inject those into the constructor. This assures that 
Managed, Singleton and Bootstrap serviecs will benefit from all configuration values. 
- IContainer, IValueInjector, EventRegistry, IConfigurationService, TypeRegistry, InstanceRegistry, IInstanceFactory, and ILogger
can be injected into configurations.
  
Here's an example of a configuration creating a database.

```
[Configuration(Priority = 100)]
public class DatabaseConfiguration
{

    [Autowire]
    public DatabaseConfiguration(IContainer container, 
        ILogger logger,
        IConfigurationService configService,
        [Value(Key = "ConnectionStrings:Db")] string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ConfigurationEntityContext>();
        if (connectionString != null)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            logger.LogInformation("Using in-memory database");
            optionsBuilder.UseInMemoryDatabase("Exemplar-Db");
        }

        var context = new ConfigurationEntityContext(optionsBuilder.Options);

        context.Database.EnsureCreated();

        container.Register(typeof(ConfigurationEntityContext), context);

        configService.AddSource(new DatabaseConfigurationSource(context));

    }
}                                   
```


### Singleton

Singletons are scanned after configurations. They are not instantiated
until requested from the container or autowired into another service.

```
[Singleton]
public class PropertyService
{
    [Autowire] private ConfigurationEntityContext context;
    /// <summary>
    /// Set a property using the configuration system.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set(string key, string value)
    {
        var configurationValue = context.ConfigurationEntity.FirstOrDefault(o => o.Name == key) ??
                                    new ConfigurationEntity
                                    {
                                        Name = key
                                    };
        
        configurationValue.Value = value;
        
        var ud = context.Entry(configurationValue).State == EntityState.Detached 
            ? context.Add(configurationValue) 
            : context.Update(configurationValue);

        context.SaveChanges();
    }
}
```

### Managed

Classes marked with the Managed attribute will register transient meaning they will be 
new instances whenever a Get<TInstance> is called (this includes when Autowiring).

```
[Managed]
public class WebClientService
{
    [Autowire] private HttpClient client;
    [Value] private string ApiUrl;

    public TEntity Get<TEntity, TRequest>(string path, TRequest request)
    {
        ... Do some Get Stuff ...
    }
}
```

### Bootstrap

Bootstrap classes are instantiated at the end of the component scanning process and
respond to OnCreate and OnStartup lifecycle events. All Managed, Singleton and Configuration
types are available during the lifecycle of a bootstrap class. 

Bootstrap classes are used to intialize the environment of a running application.

```
[Bootstrap]
public class BootstrapService
{
    [Autowire] private IConfigurationService configurationService;
    [Autowire] private PropertyService properties;

    [OnCreate]
    public void FirstRunCheck()
    {
        if (configurationService.Get("FirstRun", "false") == "false")
        {
            // Do some setup stuff
            // ....

            properties.Set("FirstRun", "true");
        }

        ...More setup for the entire app...
    }
}
```

## Lifecycle Attributes

There are numerious lifecycle attributes which determine behavior when an the container starts or an object is Created / Destroyed / etc. Additional lifecycle 
attributes can be added by inheriting the ILifecycle interface. The InstanceFactory can then be modified to use these lifecycle types. All managed types
will have OnCreate called when an instiance is created (think of it as an initializer).

## Value Injection

Values can be injected into constructors, fields and properties automatically. The source of these values depends on what is configured in the IValueInjector class on the ApplicationContext.
By default the IValueInjector will use the IConfigurationService and lookup a Key. The key can be specified on the attribute or if it is not specified the field / property / parameter name will
be used.

## Advanced Concepts

Here are a few more advanced concepts that the framework provides.

### Conditional Implementations

It is possible to feature flag implementations using a system of Conditional Singletons. 

```
interface IConditionalService
{
    void DoSomething()
}

[Singleton]
[Condition(Expression = @"Color == Green")] // No need for quotes around strings
class GreenService : IConditionalService
{
    public void DoSomething(){
        Console.WriteLine("It's Green!")
	}
}

[Singleton]
[Condition(Expression = @"Color == Blue")] // Color is a configuration key, any configuration key can be used.
class BlueService : IConditionalService
{
    public void DoSomething(){
        Console.WriteLine("It's Blue!")
	}
}

[Managed]
class ManagedService
{
    [Autowire] public IConditionalService service; // this will be GreenService when autowired.
}

/// <summary>
/// Basic configuration that will set it to the Green environment.
/// </summary>
[Configuration]
class ConfigurationSetup
{
    [Autowire]
    public ConfigurationSetup(IContainer container)
    {
        container.Get<IConfigurationService>().AddSource(new MapConfigurationSource
        {
            BackingMap = new Dictionary<string, object>
            {
                {"Color", "Green"}
            }
        });
    }
}
/// <summary>
/// An application that will use feature flagging to load two different services
/// </summary>
class FeatureFlagApplication : ApplicationContainer
{
    public FeatureFlagApplication()
    {
        
    }
}
``` 

### Application Event Pipeline

Any managed type can be an event receiver. Just mark any method with 
[OnEvent(EventType=typeof(SomeEventClass))]

```
[Singleton]
class SingletonEventReceiver {
    [Autowire] private ISomeEventClassProcessor process;

    [OnEvent(EventType=typeof(SomeEventClass))]
    public void GotEvent(IContainer container, SomeEventClass evt) {
        process.processEvent(evt)
    }
}
```

To send an event just autowire the EventRegistry in any managed type 
and you can fire those events.

```
[Managed]
class SomeEventSender {
    [Autowire] private IEventRegistry events;
    
    public void SendEvent(){
        // FireAsync is available, but is currently untested.
        events.Fire(new SomeEventClass {
            EventData = "Whatever Data You Want"
        }); // all event receivers listening for this class will get this event
       
    }
}
```

## Testing in the container
If you wish to test the application you're building from within a forge
application container, here are a few things you can do to help out.

### Configuring Test Logging
```
[Module]
class ConfigureTestLoggerModule
{
    public static ITestOutputHelper output;

    [Autowire]
    public ConfigureTestLoggerModule(ApplicationContext ctx)
    {
        ctx.Logger = new TestLogger(output).Logger;
    }
}

public class AppInContainerTests
{
    private ApplicationContainer container;
    public AppInContainerTests(ITestOutputHelper output)
    {
        ConfigureTestLoggerModule.output = output;

        container = new ApplicationContainer();
    }
}
```

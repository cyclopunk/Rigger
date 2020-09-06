![Tests](https://github.com/cyclopunk/Rigger/workflows/Test%20Rigger%20Application/badge.svg)

# Rigger

Rigger is a framework for dependency injection, inversion of control and container-managed instances. It is a part of a greater project I'm working on. It is being developed to address some feature gaps in common C# DI frameworks. Some may notice that it is very Spring Boot-esq and that is intentional. It is the hope that this platform can be used to quickly build projects with advanced features. 

Rigger is designed to be fast, lightweight, extensible and easy to use (configuration-lite). 

Rigger is also a drop-in replacement for IServiceProvider for dotnetcore projects.

### Features
+ Component Scanning
+ Attribute based registration and autowiring.
+ Configuration-based Value Injection
+ Transient, Singleton, Scopped and Thread based registrations
+ Conditional Service Registration
+ Built-in services for container-wide use (Configuration, Logging, Eventing)
+ Instance lifecycle hooks (OnCreate, OnStartup, OnBuilt, OnDestroy)
+ Application Event Pipeline
+ Cached expression-based Member Invokers and Constructor Activators for speedy autowiring.
+ Module / Plugin System for Extensibility (Drones)
+ Fail Fast application startup.
+ Managed startup lifecycle and dependency ordering (Module -> Configuration -> Managed -> Singleton -> Bootstrap).
+ Include Additional Drone libraries for added functionality :
  - Drone.Cloud, unified cloud services automatically configured and injected into the container
  - Drone.API, automatically bootstraps a GraphQL endpoint that integrates your entity model
  - Drone.Data, Wide Column Storage, ML Services, And Statistical Services
  - Drone.Chronos, Timer functionality for services.

### TODO 
I recently refactored this from another project, so there's a lot to do before I can consider this "stable".
- Update Docs
- Reintegrate Events
- Reintegrate Lifecycle management
- Reintegrate Conditional Services
- More benchmarks
- Convert modules to Rigger Drone framework
- Scoped DI needs tests (and is probably broken)


## Quickstart

To create a Rigged application, it is as simple as inheriting from the Rig class. Merely instantiating that class, or inheriting from it, will trigger
the configuration of the application. This will start the component scanning part of the framework and it will discover all components
that are marked with a Managed Type attribute. You can pass in additional assemblies to the ApplicationContainer constructor in order to 
include other Assemblies in your application. This will allow a plugin type style of development.
                        
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
        ... Code ...
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

### Bootstrap - NOT IMPLEMENTED

Bootstrap classes are instantiated at the end of the component scanning process and
are good places to put to OnCreate lifecycle events. All Managed, Singleton and Configuration
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

## Lifecycle Attributes - OnCreate Implemented, others not

There are numerious lifecycle attributes which determine behavior when an the container starts or an object is Created / Destroyed / etc. Additional lifecycle 
attributes can be added by inheriting the ILifecycle interface. The InstanceFactory can then be modified to use these lifecycle types. All managed types
will have OnCreate called when an instiance is created (think of it as an initializer).

## Value Injection

Values can be injected into constructors, fields and properties automatically. The source of these values depends on what is configured in the IValueInjector class on the ApplicationContext.
By default the IValueInjector will use the IConfigurationService and lookup a Key. The key can be specified on the attribute or if it is not specified the field / property / parameter name will
be used.

## Advanced Concepts

Here are a few more advanced concepts that the framework provides.

### Conditional Implementations - NOT IMPLEMENTED

It is possible to feature flag implementations using a system of Conditional Singletons. 

```
interface IConditionalService
{
    void DoSomething()
}

/// <summary>
/// Basic configuration that will set it to the Green environment.
/// </summary>
[Configuration]
class ConfigurationSetup
{
    public ConfigurationSetup(IServices container)
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
/// An application that will use feature flagging to load two different services
/// </summary>
class FeatureFlagApplication : Rig
{
    public FeatureFlagApplication()
    {
        
    }
}
``` 

### Application Event Pipeline

Any managed type can be an event receiver. Just mark any method with 
[OnEvent(typeof(SomeEventClass))]

```
[Singleton]
class SingletonEventReceiver {
    [Autowire] private ISomeEventClassProcessor process;

    [OnEvent(typeof(SomeEventClass))]
    public void GotEvent(SomeEventClass evt) {
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


    public ConfigureTestLoggerModule(IServices services)
    {
        services.Replace<ILoggerFactory>(new TestLogger(output).LoggerFactory)
    }
}

```

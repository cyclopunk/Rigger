![Test Rigger Application](https://github.com/cyclopunk/Rigger/workflows/Test%20Rigger%20Application/badge.svg)

# Rigger

Rigger is a framework that adds dependency injection, lifecycle managerment, application eventing and other services to a DotNet application with very minimal setup. 

It was developed to address some feature gaps that I was struggling with in C# DI frameworks, and to reduce the startup time for projects that I was working on for my job and personally. 

I owe much of the "feel" and ergonomics of Rigger to the SpringBoot project. I hope that Rigger will be used to quickly build projects and prototypes with advanced features. 

Rigger is designed to be fast, lightweight, extensible and easy to use (configuration-lite). 

Rigger is also a drop-in replacement for IServiceProvider for dotnetcore projects.

The project name and plugin scheme is inspired by the Role Playing Game Shadowrun. Riggers augment their capabilities by using autonomous drones with different features.

### Features
+ Component Scanning - Rigger does not use a single configuration file to manage DI, it scans the assembly at startup and registers all classes with special annotations.
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
+ Managed startup lifecycle and dependency ordering.
+ Include Additional Drone libraries for added functionality :
  - Drone.Cloud, unified cloud services automatically configured and injected into the container
  - Drone.API, automatically bootstraps a GraphQL endpoint that integrates your entity model
  - Drone.Data, Wide Column Storage, ML Services, And Statistical Services
  - Drone.Chronos, Timer functionality for services.
+ Extreme framework flexability - All basic functionality can be replaced by using Replace<OldImpl, NewImpl>() on the Rig. Here are some basic services with default implementations:
    - IAutowirer - Autowires an instance by looking up services and injecting them into fields or properties.
    - IConfigurationService - A container wide configuration service
    - IMethodInvoker - Invokes a method with parameters from the container.
    - IEventRegistry - Registers managed instances as event receivers and dispatches events
    - IInstanceFactory - Create's fully managed instances (Autowired, Value Injected, Fast Constructed by default)
    - IConstructorActivator - Finds the most appropriate constructor and tries to create an object using  it
    - IValueInjector - Scans a class for [Value] attributes and automatically inserts them where possible
+ POCOs - Currently there are no proxies being used. Types that are managed by the container are merely POCOs with Attributes.


### TODO 

- Class protection level changes
- Example projects.
- More testing
- Benchmarking 
- Re-integration of drones.
- Container validation (circular depdendnecies, conditional issues)


## Quickstart

To create a Rigged application, it is as simple as inheriting from the Rig class. Merely instantiating that class, or inheriting from it, will trigger
the configuration of the application. This will start the component scanning part of the framework and it will discover all components
that are marked with a Rigger class attribute. You can pass in additional assemblies or namespace strings to the Rig constructor in order to 
include other Assemblies in your application. This will allow a plugin type style of development.

In order to use it with Asp.Net Core, just call `.UseServiceProviderFactory(new RiggedServiceProviderFactory())` on your IHostBuilder. And make sure to .AddControllersAsServices() if you want to use [Autowire] and [Value].
                        
### Singleton

Singletons are scanned after modules. They are not instantiated
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

### Bootstrap 

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

## Conditional Implementations

It is possible to feature flag implementations using the Condition attribute. Currently these conditions are set up upon registration / scanning. So once a service
is instantiated once it will continue to be that service for the lifetime of the scope (which may be the application lifetime).

It is possible to support hot-swapping of configuration values, but the default implementation does not support this.

```
interface IConditionalService
{
    void GetColor()
}

[Singleton]
[Condition(@"ServiceColor == Green")] // No need for quotes around strings
class GreenService : IConditionalService
{
    public string GetColor(){
        return "Green";
	}
}

[Singleton]
[Condition(@"ServiceColor == Blue")] // Color is a configuration key, any configuration key can be used.
class BlueService : IConditionalService
{
    public string GetColor(){
        return "Blue";
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
       Environment.SetEnvironmentVariable("ServiceColor", "Green");
    }
}
``` 

## Application Event Pipeline

Any managed type can be an event receiver. Just mark any method with `[OnEvent(typeof(SomeEventClass))]`. Note: Only transient objects that are still alive will receive this message.

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

Testing can be acommplished inside and outside the Rigged application. Since managed types are just POCOs
Unit testing should be a breeze. If you want to mock services or install your own defaults you can use the "Replace" 
method on services.

You can use a Drone namespace to add new managed components to your tests as well.

```
namespace Drone.MyTests {
    [Singleton]
    public class SingletonService {

    }
}

[Fact]
public void MyTest {
    Rig rig = new Rig("Drone.MyTests")

    var service = rig.Get<SingletonService>(); // this should not be null
}
```

### Configuring Test Logging

Rigger comes with a test logger for xUnit. Just add this module to your test Drone namespace. This will allow
the Rigger log output to be displayed in the ITestOutputHelper.

```
namespace Drone.MyTests {
    public static ITestOutputHelper output;
    [Module(Priority=1)]
    public class ChangeLogger {

        // services are automatically injected into all module constructors.
        public ChangeLogger(IServices services){
            services.Replace<ILoggerFactory>(new TestLogger(output).LoggerFactory)
        }
    }
}

public class xUnitTests {
    public xUnitTests(ITestOutputHelper output){
        ChangeLogger.output = output;        
    }
    [Fact]
    public void MyTest {
        Rig rig = new Rig("Drone.MyTests")

        var testLogger = rig.Get<ILogger<xUnitTests>>();
    }
}
```

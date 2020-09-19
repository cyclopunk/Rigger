using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rigger.Extensions;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.Attributes;
using Rigger.Exceptions;
using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.ComponentScanners;
using Rigger.Reflection;

namespace Rigger {
    /// <summary>
   /// Top level class for the Rigger application container. The container will hold all of the
   /// managed types and manage their lifetime as well as provide a component scanning
   /// functionality that will allow attributes to be used to manage objects.
   ///
   /// Application containers should be implemented for whatever DI framework is being used for
   /// the project.
   /// </summary>
    public class Rig : IContainer
   {

       private static string PLUGIN_NAMESPACE = "Drone";

       private bool _disposing = false;

       private ILogger<Rig> logger;
       private IEventRegistry eventRegistry;

       public IServices Services { get; set; }

       public TInstance Get<TInstance>(params object[] constructorParams) where TInstance : class
       {
           return Services.GetService<TInstance>();
       }

       public object Get(Type type, params object[] constructorParams)
       {
           return Services.GetService(type);
       }

       public IContainer Register(Type type, object instance)
       {
           Services.Add(type, instance);

           eventRegistry?.Register(instance);

           return this;
       }

       public IContainer Register<TType>(object instance)
       {
           Services.Add<TType>(instance);

           eventRegistry?.Register(instance);

           return this;
       }

       public IContainer Register<TInterface, TConcrete>(ServiceLifetime type = ServiceLifetime.Singleton)
       {
           Services.Add<TInterface,TConcrete>(type);

           return this;
       }

       public IContainer Register(Type lookupType, Type implType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            Services.Add(lookupType, implType, lifetime);

            return this;
        }

       /// <summary>
       /// Register a service and return an instance of that service.
       /// </summary>
       /// <typeparam name="TManagedType"></typeparam>
       /// <param name="constructorParams"></param>
       /// <returns></returns>
        public TManagedType RegisterAndGet<TManagedType>(params object[] constructorParams) where TManagedType : class
       {
           return Services.Add<TManagedType, TManagedType>().GetService<TManagedType>();
       }

       public bool IsBuilt { get; protected set; }
        
       /// <summary>
       /// This method will return the default assemblies.
       /// </summary>
       /// <returns></returns>
       private Assembly[] DefaultAssemblies()
       {
           var executingAssembly = Assembly.GetExecutingAssembly();

           List<Assembly> autoScannedAssemblies = new List<Assembly>();
           
           autoScannedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
               .Where(o => o.FullName.Contains(PLUGIN_NAMESPACE)));

           autoScannedAssemblies.Add(GetType().Assembly);
           // ServiceLifetime attributes from Traits
           autoScannedAssemblies.Add(typeof(ILifecycle).Assembly);

           //logger.LogInformation($"Default assemblies: {string.Join(",",autoScannedAssemblies.Select(o=>o.FullName))}");

           return autoScannedAssemblies.Distinct().ToArray();
       }

       private IEnumerable<Assembly> SearchAppDomain(string searchString)
       {
           AddDynamicAssemblies(searchString);

           return AppDomain.CurrentDomain.GetAssemblies()
               .Where(o => o.FullName.Contains(searchString));
       }

       /// <summary>
       /// Since assemblies are not added right away we have to load
       /// them from the app domain directory.
       ///
       /// This method will load the ones matching the namespace string.
       /// </summary>
       /// <param name="namespaceString">The namespace string to match.</param>
       private Assembly[] AddDynamicAssemblies(string namespaceString)
       {
           var path = AppDomain.CurrentDomain.BaseDirectory;

           Directory.GetFiles(path, "*.dll")
               .Where(o =>
               {
                   var filename = Path.GetFileName(o);

                   return  filename.StartsWith(namespaceString);
               })
               .ForEach(o =>
               {
                   Assembly.LoadFrom(o);
               });

          return AppDomain.CurrentDomain.GetAssemblies();
       }

       /// <summary>
       /// Create an application scanner that will scan the default Assemblies (Any assembly that has TheCommons.Rig in the name
       /// as well as the executing assembly for components.
       /// </summary>
       /// <param name="assemblies"></param>
       public Rig(Assembly[] assemblies)
       {

           if (!assemblies.Any() && this.GetType().GetCustomAttribute<ContainerOptionsAttribute>()?.Empty == true) return;

           Build(DefaultAssemblies().Concat(assemblies).ToArray());
       }


       public Rig(params string[] namespaceStrings)
       {
           Console.WriteLine($"Searching domain {namespaceStrings} for Drones.");

           var namespaceAssemblies = namespaceStrings.Select(SearchAppDomain).Combine();

           Build(DefaultAssemblies().Concat(namespaceAssemblies).ToArray()); 
       }

       /// <summary>
        /// Build the application container from the assembly. This will find all modules, run all component scanners that have been discovered
        /// and initialize the event registry.
        /// </summary>
        /// <param name="assemblies">The assemblies that will be used to build the application.</param>
        public void Build(params Assembly[] assemblies)
       {

            Services = new Services();

            Services.Add<IServiceProvider>(this);
            // bootstrap by adding the module

            Services
                .Add<ModuleComponentScanner, ModuleComponentScanner>(ServiceLifetime.Singleton)
                .Add<IContainer>(this)
                .GetService<ModuleComponentScanner>()
                .ComponentScan(assemblies);

            logger = Services.GetService<ILogger<Rig>>();

            if (logger == null)
            {
                throw new Exception("No ILogger<> registered, please register an ILogger with the container as it is a guaranteed service.");
            }
            
            var scanners = Services.GetService<IEnumerable<IComponentScanner>>(CallSiteType.Method).ToList();

            if (!scanners.Any())
            {
                throw new ContainerException("No component scanners identified, that most likely means the Default Module was not found. Install the Drone.Bootstrap nuget package to continue, or build your own module.");
            }

            scanners.ForEach(o =>
            {
                try
                {
                    o.ComponentScan(assemblies);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Could not build component with scanner {o}");
                    throw;
                }
            });

            eventRegistry = Services.GetService<IEventRegistry>();

            if (eventRegistry == null)
            {
                logger.LogWarning("No event registry found, the Application Event Pipeline will not be available for this application.");
            }

            // other basic checks

            if (!Services.IsManaged<IConstructorActivator>())
            {
                logger.LogWarning("An IConstructorActivator was not found, object construction will be slower without one.");
            }

            if (!Services.IsManaged<IAutowirer>())
            {
                logger.LogWarning("An autowirerer was not found, Autowiring services will not be available for this application.");
            }

            IsBuilt = true;
        }

       /// <summary>
        /// Method that is called when the application is disposed of. Clears all resources,
        /// instances, caches, etc that are associated with the container. Calls the OnDestroy hook
        /// of all managed instances.
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;
            Services.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;

            await Services.DisposeAsync();

            await Task.CompletedTask;
        }

        public object GetService(Type serviceType)
        {
            var instance = Services.GetService(serviceType);

            Services.GetService<IEventRegistry>()?.Register(instance);

            return instance;
        }

        public IContainer Register(Type lookupType, Func<IServices, object> factory, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            Services.Add(lookupType, factory, serviceLifetime);

            return this;
        }
    }
}
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
using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.ComponentScanners;

namespace Rigger {
    /// <summary>
   /// Top level class for an application container. The container will hold all of the
   /// managed types and manage their lifecycle as well as provide a component scanning
   /// functionality that will allow attributes to be used to manage objects.
   ///
   /// Application containers should be implemented for whatever DI framework is being used for
   /// the project. A SimpleInjector example is provided with this library as a reference
   /// implementation.
   /// </summary>
    public class Rig : IContainer
   {

       private static string PLUGIN_NAMESPACE = "Drone";

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

           Services.GetService<IEventRegistry>()?.Register(instance);

           return this;
       }

       public IContainer Register<TType>(object instance)
       {
           Services.Add<TType>(instance);

           Services.GetService<IEventRegistry>()?.Register(instance);

           return this;
       }

       public IContainer Register<TInterface, TConcrete>(ServiceLifecycle type = ServiceLifecycle.Singleton)
       {
           Services.Add<TInterface,TConcrete>(type);

           return this;
       }

       public TManagedType RegisterAndGet<TManagedType>(params object[] constructorParams) where TManagedType : class
       {
           return Services.Add<TManagedType, TManagedType>().GetService<TManagedType>();
       }

       public bool IsBuilt { get; protected set; }
        
       private Assembly[] DefaultAssemblies()
       {
           var executingAssembly = Assembly.GetExecutingAssembly();

           List<Assembly> autoScannedAssemblies = new List<Assembly>();
           
           autoScannedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
               .FindAll(o => o.FullName.Contains(PLUGIN_NAMESPACE)));

           autoScannedAssemblies.Add(this.GetType().Assembly);
           // ServiceLifecycle attributes from Traits
           autoScannedAssemblies.Add(typeof(ILifecycle).Assembly);

           //logger.LogInformation($"Default assemblies: {string.Join(",",autoScannedAssemblies.Select(o=>o.FullName))}");

           return autoScannedAssemblies.Distinct().ToArray();
       }

       private Assembly[] SearchAppDomain(string searchString)
       {
           AddDynamicAssemblies(searchString);

           return AppDomain.CurrentDomain.GetAssemblies()
               .FindAll(o => o.FullName.Contains(searchString));
       }

       /// <summary>
       /// Since assemblies are not added right away we have to load
       /// them from the app domain directory. We'll only load the ones matching the namespace string though.
       /// </summary>
       /// <param name="namespaceString"></param>
       private void AddDynamicAssemblies(string namespaceString)
       {
           var path = AppDomain.CurrentDomain.BaseDirectory;

           //logger.LogInformation($"Searching app directory {path} for DLLs starting with " + namespaceString);

           Directory.GetFiles(path, "*.dll")
               .Where(o =>
               {
                   var filename = Path.GetFileName(o);

                   //logger.LogInformation($"Found {filename} from {o}");

                   return  filename.StartsWith(namespaceString);
               })
               .ForEach(o =>
               {
                   //logger.LogInformation($"Loading assembly.");
                   Assembly.LoadFrom(o);
               });

           var domain = AppDomain.CurrentDomain.GetAssemblies();

           //logger.LogInformation($"Domain assemblies: {string.Join(",",domain.Select(o => o.FullName))})");

       }

       /// <summary>
       /// Create an application scanner that will scan the default Assemblies (Any assembly that has TheCommons.Rig in the name
       /// as well as the executing assembly for components.
       /// </summary>
       /// <param name="assemblies"></param>
       public Rig(params Assembly[] assemblies)
       {

           if (!assemblies.Any() && this.GetType().GetCustomAttribute<ContainerOptionsAttribute>()?.Empty == true) return;

           Build(DefaultAssemblies().Concat(assemblies).ToArray());
       }


       public Rig(string namespaceString)
       {
           //logger.LogInformation($"Searching domain {namespaceString} for Forge artifacts.");
           Build(DefaultAssemblies().Concat(SearchAppDomain(namespaceString)).ToArray()); 
       }

       /// <summary>
        /// Build the application container from the assembly. This will initialize the default context, scan the assembly and then validate
        /// that the application is complete.
        /// </summary>
        /// <param name="assemblies">The assemblies that will be used to build the application.</param>
        public void Build(params Assembly[] assemblies)
       {

            Services = new Services();

            // bootstrap by adding the module

            Services
                .Add<ModuleComponentScanner, ModuleComponentScanner>(ServiceLifecycle.Singleton)
                .GetService<ModuleComponentScanner>()
                .ComponentScan(assemblies);

            var logger = Services.GetService<ILogger<Rig>>();
            
            var scanners = Services.GetService<IEnumerable<IComponentScanner>>();

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

            IsBuilt = true;
        }

       /// <summary>
        /// Method that is called when the application is disposed of. Clears all resources,
        /// instances, caches, etc that are associated with the container. Calls the OnDestroy hook
        /// of all managed instances.
        /// </summary>
        public virtual void Dispose()
        {
            Services.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await Services.DisposeAsync();

            await Task.CompletedTask;
        }

        public object GetService(Type serviceType)
        {
            var instance = Services.GetService(serviceType);

            Services.GetService<IEventRegistry>()?.Register(instance);

            return instance;
        }
   }
}
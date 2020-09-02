using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheCommons.Core.Extensions;
using TheCommons.Core.Reflection;
using TheCommons.Forge.Events;
using TheCommons.Forge.Exceptions;
using TheCommons.Forge.ManagedTypes.ComponentHandlers;
using TheCommons.Forge.ManagedTypes.ComponentScanners;
using TheCommons.Traits.Attributes;
using TheCommons.Forge.ManagedTypes.Implementations;
using TheCommons.Forge.ManagedTypes.Implmentors;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Forge.ManagedTypes.ServiceLocator;
using TheCommons.Logging;

namespace TheCommons.Forge.ManagedTypes
{
   /// <summary>
   /// Top level class for an application container. The container will hold all of the
   /// managed types and manage their lifecycle as well as provide a component scanning
   /// functionality that will allow attributes to be used to manage objects.
   ///
   /// Application containers should be implemented for whatever DI framework is being used for
   /// the project. A SimpleInjector example is provided with this library as a reference
   /// implementation.
   /// </summary>
    public class Crucible : IContainer
   {
       ILogger logger = new ConsoleLogger().Logger;

       private static string PLUGIN_NAMESPACE = "TheCommons.Forge";

       public Lightweight.Services Services { get; set; }
       public Assembly[] ScannedAssemblies { get; set; }
       public Type[] Ingots { get; protected set; }

       public bool IsValidated { get; protected set; }
       public Services services { get; set; }

       public object Get(Guid scopeId, Type lookupType, params object[] constructorParams)
       {
           throw new NotImplementedException();
       }

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

           return this;
       }

       public IContainer Register<TType>(object instance)
       {
           Services.Add<TType>(instance);

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

           logger.LogInformation($"Default assemblies: {string.Join(",",autoScannedAssemblies.Select(o=>o.FullName))}");

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

           logger.LogInformation($"Searching app directory {path} for DLLs starting with " + namespaceString);

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

           logger.LogInformation($"Domain assemblies: {string.Join(",",domain.Select(o => o.FullName))})");

       }

       /// <summary>
       /// Create an application scanner that will scan the default Assemblies (Any assembly that has TheCommons.Crucible in the name
       /// as well as the executing assembly for components.
       /// </summary>
       /// <param name="assemblies"></param>
       public Crucible(params Assembly[] assemblies)
       {

           if (!assemblies.Any() && this.GetType().GetCustomAttribute<ContainerOptionsAttribute>()?.Empty == true) return;

           Build(DefaultAssemblies().Concat(assemblies).ToArray());
       }

       /// <summary>
       /// Create an application container that will load the provided ingot.
       ///
       /// This will autobuild the container with the ingot and the default assemblies from all
       /// Crucible modules and the executing / calling assembly.
       /// 
       /// </summary>
       /// <param name="ingotTypes">An Ingot to load</param>
       public Crucible(Type ingotTypes) : this(new []{ ingotTypes })
       {

       }

       /// <summary>
       /// Builds an application container with the ingots loaded.
       ///
       /// This will autobuild the container with the ingot and the default assemblies from all
       /// Crucible modules and the executing / calling assembly.
       /// </summary>
       /// <param name="ingotTypes"></param>
       public Crucible(Type[] ingotTypes)
       {
           Ingots = ingotTypes;
           // Build with the basic stack
           Build(DefaultAssemblies()); 

           ingotTypes.ForEach(LoadIngot);
       }

       public Crucible(string namespaceString)
       {
           logger.LogInformation($"Searching domain {namespaceString} for Forge artifacts.");
           Build(DefaultAssemblies().Concat(SearchAppDomain(namespaceString)).ToArray()); 
       }

       /// <summary>
        /// Build the application container from the assembly. This will initialize the default context, scan the assembly and then validate
        /// that the application is complete.
        /// </summary>
        /// <param name="assemblies">The assemblies that will be used to build the application.</param>
        public void Build(params Assembly[] assemblies)
        {
            var logger = Services.GetService<ILogger<Crucible>>();

            logger.LogInformation($"[FORGE] Scanning the following assemblies: {string.Join(",", assemblies.Select(o => o.FullName))}");

            ScannedAssemblies = assemblies;

            // scan the assemblies and ingots for the rest of the application.
            
           
            IsBuilt = true;

            // TODO Add component scanning back

            logger.LogInformation($"FORGE: Scanned the following assemblies: {string.Join(",", assemblies.Select(o => o.FullName))}");

            logger.LogInformation($"FORGE: Validating the container.");

            // TODO validation logic

            logger.LogInformation($"FORGE: Firing OnBuilt for all Managed Types");
            
            IsValidated = true;
        }

        public void LoadIngot(Type t)
        {
           
        }

        public void LoadIngot<TIngotType>()
        {
            LoadIngot(typeof(TIngotType));
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
            return Services.Get(serviceType);
        }
   }
}
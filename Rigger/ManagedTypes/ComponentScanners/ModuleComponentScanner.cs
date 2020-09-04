using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Rigger.Extensions;
using Rigger.Dependencies;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes;

namespace Rigger.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Component scanner that will handle bootstrap classes (Lifecyces, Bootstraps, Configuration)
    /// </summary>
    public class ModuleComponentScanner : IComponentScanner
    {
        public IServices Services { get; set; }

        [Autowire] private ILogger<ModuleComponentScanner> _logger;

        public ModuleComponentScanner(IServices services)
        {
            this.Services = services;
        }

        public IEnumerable<Type> ComponentScan(params Assembly[] assemblies)
        {

            assemblies
                .Select(a =>  a.TypesWithAttribute<ModuleAttribute>() )
                .Combine()
                .Where(w => !w.IsNested) // No nested types, these are ingots and need to be loaded manually
                .OrderByDescending(x => x.GetCustomAttribute<ModuleAttribute>().Priority)
                .ForEach(c =>
                {
                    Activator.CreateInstance(c, Services);
                });

            
            return null;
        }
    }
}
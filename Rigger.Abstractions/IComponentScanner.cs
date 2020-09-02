using System;
using System.Reflection;
using Rigger.Injection;

namespace Rigger.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// The scanner interface is implemented on types that will provide assembly scanning and
    /// DI capabilities.
    /// </summary>
    /// <typeparam name="T">T is the type of the container that will be returned by the component scan.</typeparam>
    public interface IComponentScanner <T> : IServiceAware where T : class
    {
        /// <summary>
        /// LoadModules an array of assemblies for components and return the result (normally a container or enumeration)
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>A component that will manage the components that have been scanned.</returns>
        T ComponentScan(params Assembly[] assemblies);
    }
}

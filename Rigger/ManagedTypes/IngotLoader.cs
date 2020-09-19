using System;
using System.Linq;
using System.Reflection;
using Rigger.Extensions;
using Rigger.Reflection;
using Rigger.Exceptions;
using Rigger.Attributes;
using Rigger.Injection;

namespace Rigger.ManagedTypes
{
    /// <summary>
    /// Class with helper methods for loading Ingots.
    /// </summary>
    public class IngotLoader
    {
        private Type _type;
        public IngotLoader(Type type)
        {
            if (type.GetNestedTypes(ReflectionExtensions.defaultBindingFlags).Length == 0)
            {
                throw new ContainerException($"Ingot {type} has no nested types, cannot load.");
            }

            _type = type;
        }
        /// <summary>
        /// Load all modules from a context.
        /// </summary>
        /// <param name="context">The application context to load.</param>
        public void LoadModules(Services services)
        {
            _type.GetNestedTypes().Where(t => t.GetCustomAttribute<ModuleAttribute>() != null)
                .OrderBy(t => t.GetCustomAttribute<ModuleAttribute>().Priority)
                .ForEach(nt =>
            {
                if (nt.GetConstructor(new Type[] { typeof(Services) }) == null) return;

                var constructorInvoker = new ExpressionActivator(nt);

                var module = constructorInvoker.Activate(services);
            });
        }
    }
}
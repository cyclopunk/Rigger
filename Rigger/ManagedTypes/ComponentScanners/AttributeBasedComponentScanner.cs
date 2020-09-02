using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rigger.Extensions;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes;

namespace Rigger.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Component scanner that will will scan for all types that have TAttribute. The scan
    /// will return a list of the types that it found and also find the component handler for
    /// that attribute if it is registered with the container.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    public class AttributeBasedComponentScanner<TAttribute> : IComponentScanner where TAttribute : Attribute
    {

        public IServices Services { get; set; }

        public IEnumerable<Type> ComponentScan(params Assembly[] assemblies)
        {
            var handler = Services.GetService<IComponentHandler<TAttribute>>();

            var foundAttributeTypes = assemblies.Map(assembly => assembly.TypesWithAttribute<TAttribute>())
                .Combine().ToList();

            foundAttributeTypes.ForEach(handler.HandleComponent);

            return foundAttributeTypes;
        }
    }
}
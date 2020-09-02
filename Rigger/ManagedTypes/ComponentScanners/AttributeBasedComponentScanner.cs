using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheCommons.Core.Extensions;
using TheCommons.Forge.ManagedTypes.ComponentHandlers;
using TheCommons.Traits.Attributes;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Lightweight;

namespace TheCommons.Forge.ManagedTypes.ComponentScanners
{
    /// <summary>
    /// Component scanner that will will scan for all types that have TAttribute. The scan
    /// will return a list of the types that it found and also find the component handler for
    /// that attribute if it is registered with the container.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    public class AttributeBasedComponentScanner<TAttribute> : IComponentScanner<List<Type>> where TAttribute : Attribute
    {

        public Services Services { get; set; }

        [Autowire] private IContainer _container;
        public List<Type> ComponentScan(params Assembly[] assemblies)
        {
            var handler = _container.Get<IComponentHandler<TAttribute>>();

            var foundAttributeTypes = assemblies.Map(assembly => assembly.TypesWithAttribute<TAttribute>())
                .Combine().ToList();

            foundAttributeTypes.ForEach(handler.HandleComponent);

            return foundAttributeTypes;
        }
    }
}
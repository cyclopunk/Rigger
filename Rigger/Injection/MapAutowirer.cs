using System;
using System.Collections.Generic;
using Rigger.Extensions;
using Rigger.Exceptions;
using Rigger.Attributes;

namespace Rigger.Injection
{
    /// <summary>
    /// A simple autowirer that uses a map to inject instances into
    /// classes. This class can be used in tests to autowire properties
    /// without using the managed type container.
    /// </summary>
    public class MapAutowirer : IAutowirer
    {

        public Dictionary<Type, object> Map = new Dictionary<Type, object>();

        public TRType Inject<TRType>(TRType objectToInjectTo)
        {
            // inject into properties
            objectToInjectTo.GetType().PropertyWithAttribute<AutowireAttribute>().ForEach(property =>
            {
                // get an instance of the type that is being autowired.
                Map.TryGetValue(property.PropertyType, out var instance);

                if (instance == null)
                {
                    throw new ManagedTypeNotRegisteredException(
                        $"Managed type {property.PropertyType} not registered in the map.");
                }

                property.SetValue(objectToInjectTo, instance);
            });

            // inject into fields
            objectToInjectTo
                .GetType()
                .FieldsWithAttribute<AutowireAttribute>()
                .ForEach(field =>
                {
                    Map.TryGetValue(field.FieldType, out var instance);

                    field.SetValue(objectToInjectTo, instance);
                });

            return objectToInjectTo;
        }
    }
}
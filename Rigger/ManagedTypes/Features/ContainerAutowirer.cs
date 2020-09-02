using Rigger.Extensions;
using Rigger.Exceptions;
using Rigger.ManagedTypes.Lightweight;
using Rigger.Attributes;

namespace Rigger.ManagedTypes.Features
{
    /// <summary>
    /// Class that will preform autowiring by iterating through fields and properties and finding the [Autowire] attribute
    /// </summary>
    public class ContainerAutowirer : IAutowirer, IServiceAware
    {
        [Autowire]
        private IContainer _container;
        public Services Services { get; set; }

        public TRType Inject<TRType>(TRType objectToInjectTo)
        {
            // inject into properties
            objectToInjectTo.GetType().PropertyWithAttribute<AutowireAttribute>().ForEach(property =>
            {
                // get an instance of the type that is being autowired.
                var instance = Services.GetService(property.PropertyType) ?? _container.Get(property.PropertyType);

                if (instance == null)
                {
                    throw new ManagedTypeNotRegisteredException($"Cannot autowire {property.Name} on {typeof(TRType)}. Managed type {property.PropertyType} not registered in the container.");
                }

                property.SetValue(objectToInjectTo, instance);
            });

            // inject into fields
            objectToInjectTo
                .GetType()
                .FieldsWithAttribute<AutowireAttribute>()
                .ForEach(field =>
                {
                    var instance = Services.GetService(field.FieldType) ?? _container.Get(field.FieldType);

                    if (instance == null)
                    {
                        throw new ManagedTypeNotRegisteredException($"Cannot autowire {field.Name} on {typeof(TRType)}.Managed type {field.FieldType} not registered in the container.");
                    }

                    field.SetValue(objectToInjectTo, instance);
                });

            return objectToInjectTo;
        }

    }
}
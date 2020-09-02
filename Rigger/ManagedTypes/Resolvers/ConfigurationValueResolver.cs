using System;
using System.Reflection;
using TheCommons.Core.Configuration;
using TheCommons.Core.Extensions;
using TheCommons.Core.ValueConverters;
using TheCommons.Forge.Exceptions;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.Resolvers
{
    public class ConfigurationValueResolver : IValueResolver
    {
        [Autowire] private IContainer container;
        [Autowire] private IConfigurationService configuration;
        public ConfigurationValueResolver()
        {

        }
        public void Resolve(object instance)
        {
            if (instance == null)
            {
                return;
            }

            Type t = instance.GetType();

            t.PropertyWithAttribute<ValueAttribute>()
                .ForEach(o =>
                {
                    try
                    {
                        ValueAttribute v = o.GetCustomAttribute<ValueAttribute>();

                        var value = configuration.Get(v.Key ?? o.Name, o.GetValue(instance));

                        if (value.GetType() != o.PropertyType)
                        {
                            Type valueConverterType = typeof(IValueConverter<,>);
                            
                            IValueConverter converter = (IValueConverter) container.Get(valueConverterType.MakeGenericType(value.GetType(), o.PropertyType));

                            value = converter.Convert(value);
                        }

                        o.SetValue(instance, value);
                    } catch (Exception e)
                    {
                        throw new ContainerException($"Property value injection failed: {t} for property {o.Name} - {e.Message}");
                    }
                });

            t.FieldsWithAttribute<ValueAttribute>()
                .ForEach(o =>
                {
                    try {
                        ValueAttribute v = o.GetCustomAttribute<ValueAttribute>();

                        var value = configuration.Get(v.Key ?? o.Name, o.GetValue(instance));
                        
                        if (value.GetType() != o.FieldType)
                        {
                            Type valueConverterType = typeof(IValueConverter<,>);
                            
                            IValueConverter converter = (IValueConverter) container.Get(valueConverterType.MakeGenericType(value.GetType(), o.FieldType));

                            value = converter.Convert(value);
                        }

                        o.SetValue(instance, value);
                    } catch (Exception e) {
                        throw new ContainerException($"Field value injection failed: type: {t} for field {o.Name} - {e.Message}");
                    }
                });
        }
    }
}
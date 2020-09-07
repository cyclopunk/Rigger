using Rigger.Attributes;
using Rigger.Configuration;
using Rigger.Extensions;
using Rigger.Reflection;
using Rigger.ValueConverters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rigger.Injection.Defaults
{

    public class DefaultValueInjector : IValueInjector
    {
        public IServices Services { get; set; }

        /// <summary>
        /// Thread safe caches for expression types.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, ConcurrentBag<ExpressionPropertyAccessor>> PropertyCache =
            new ConcurrentDictionary<Type, ConcurrentBag<ExpressionPropertyAccessor>>();
        private static readonly ConcurrentDictionary<Type, ConcurrentBag<ExpressionFieldAccessor>> FieldCache =
            new ConcurrentDictionary<Type, ConcurrentBag<ExpressionFieldAccessor>>();
      
        /// <summary>
        /// This method will cache the expression accessors for all of the types that are
        /// passed into this value injector. This should speed up the value injection for a
        /// small amount of memory cost.
        /// </summary>
        /// <param name="type"></param>
        private void CacheType(Type type)
        {
            type.PropertyWithAttribute<ValueAttribute>().ForEach(property =>
            {
                var accessor = new ExpressionPropertyAccessor(property);
                PropertyCache.AddOrUpdate(type, new ConcurrentBag<ExpressionPropertyAccessor> { accessor }, (key, value) =>
                {
                    value.Add(accessor);
                    return value;
                });
            });

            // inject into fields
            type.FieldsWithAttribute<ValueAttribute>()
                .ForEach(field =>
                {
                    var accessor = new ExpressionFieldAccessor(field);

                    FieldCache.AddOrUpdate(type, new ConcurrentBag<ExpressionFieldAccessor> { accessor }, (key, value) =>
                    {
                        value.Add(accessor);
                        return value;
                    });
                });
        }
        public TRType Inject<TRType>(TRType obj)
        {
            Type type = obj.GetType();

            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (!PropertyCache.ContainsKey(type))
            {
                CacheType(type);
            }

            if (!Services.IsManaged<IConfigurationService>())
            {
                return obj;
            }

            // no values, return early.
            if (!PropertyCache.ContainsKey(type) && !FieldCache.ContainsKey(type))
            {
                return obj;
            }

            var config = Services.GetService<IConfigurationService>();
           
            if (PropertyCache.ContainsKey(type))
                PropertyCache[type].ForEach(info => {
                    ValueAttribute v = info.Property.GetCustomAttribute<ValueAttribute>();

                    var value = config.Get(v.Key ?? info.Property.Name, info.GetValue(obj));

                    if (value.GetType() != info.PropertyType)
                    {
                        var valueConverterType = typeof(IValueConverter<,>);

                        var converter = (IValueConverter)Services.GetService(valueConverterType.MakeGenericType(value.GetType(), info.PropertyType));

                        value = converter.Convert(value);
                    }

                    info.SetValue(obj, value);
                 });
            if (FieldCache.ContainsKey(type))
                FieldCache[type].ForEach(info => {
                    ValueAttribute v = info.Field.GetCustomAttribute<ValueAttribute>();

                    var value = config.Get(v.Key ?? info.Field.Name, info.GetValue(obj));

                    if (value.GetType() != info.FieldType)
                    {
                        var valueConverterType = typeof(IValueConverter<,>);

                        var converter = (IValueConverter)Services.GetService(valueConverterType.MakeGenericType(value.GetType(), info.FieldType));

                        value = converter.Convert(value);
                    }
                    info.SetValue(obj, value);
                });

            return obj;
        }
    }
}

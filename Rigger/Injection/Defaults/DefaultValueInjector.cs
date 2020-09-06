using Rigger.Attributes;
using Rigger.Configuration;
using Rigger.Extensions;
using Rigger.Reflection;
using Rigger.ValueConverters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rigger.Injection.Defaults
{

    public class DefaultValueInjector : IValueInjector
    {
        public IServices Services { get; set; }

        private static ConcurrentDictionary<Type, ConcurrentBag<ExpressionPropertyAccessor>> _cache =
            new ConcurrentDictionary<Type, ConcurrentBag<ExpressionPropertyAccessor>>();
        private static ConcurrentDictionary<Type, ConcurrentBag<ExpressionFieldAccessor>> _fieldCache =
            new ConcurrentDictionary<Type, ConcurrentBag<ExpressionFieldAccessor>>();
      
        public void CacheType(Type type)
        {
            type.PropertyWithAttribute<ValueAttribute>().ForEach(property =>
            {
                var accessor = new ExpressionPropertyAccessor(property);
                _cache.AddOrUpdate(type, new ConcurrentBag<ExpressionPropertyAccessor> { accessor }, (key, value) =>
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

                    _fieldCache.AddOrUpdate(type, new ConcurrentBag<ExpressionFieldAccessor> { accessor }, (key, value) =>
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

            if (!_cache.ContainsKey(type))
            {
                CacheType(type);
            }

            var config = Services.GetService<IConfigurationService>();
           
            if (_cache.ContainsKey(type))
                _cache[type].ForEach(info => {
                    ValueAttribute v = info.Property.GetCustomAttribute<ValueAttribute>();

                    var value = config.Get(v.Key ?? info.Property.Name, info.GetValue(obj));

                    // convert if possible, might be better using a ConvertExpression here?

                    if (value.GetType() != info.PropertyType)
                    {
                        Type valueConverterType = typeof(IValueConverter<,>);

                        IValueConverter converter = (IValueConverter)Services.GetService(valueConverterType.MakeGenericType(value.GetType(), info.PropertyType));

                        value = converter.Convert(value);
                    }
                    info.SetValue(obj, value);
                 });
            if (_fieldCache.ContainsKey(type))
                _fieldCache[type].ForEach(info => {
                    ValueAttribute v = info.Field.GetCustomAttribute<ValueAttribute>();

                    var value = config.Get(v.Key ?? info.Field.Name, info.GetValue(obj));

                    // convert if possible, might be better using a ConvertExpression here?

                    if (value.GetType() != info.FieldType)
                    {
                        Type valueConverterType = typeof(IValueConverter<,>);

                        IValueConverter converter = (IValueConverter)Services.GetService(valueConverterType.MakeGenericType(value.GetType(), info.FieldType));

                        value = converter.Convert(value);
                    }
                    info.SetValue(obj, value);
                });

            return obj;
        }
    }
}

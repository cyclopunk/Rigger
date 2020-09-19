using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rigger.Extensions;
using Rigger.Exceptions;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.Reflection;

namespace Rigger.Injection
{
    /// <summary>
    /// Class that will preform autowiring by iterating through fields and properties and finding the [Autowire] attribute
    /// </summary>
    public class ContainerAutowirer : IAutowirer, IServiceAware
    {
        private static ConcurrentDictionary<Type, ConcurrentBag<ExpressionPropertyAccessor>> _cache =
            new ConcurrentDictionary<Type, ConcurrentBag<ExpressionPropertyAccessor>>(); 
        private static ConcurrentDictionary<Type, ConcurrentBag<ExpressionFieldAccessor>> _fieldCache =
            new ConcurrentDictionary<Type, ConcurrentBag<ExpressionFieldAccessor>>();
        public IServices Services { get; set; }

        public void CacheType(Type type)
        {
            type.PropertiesWithAttribute<AutowireAttribute>().ForEach(property =>
            {
                var accessor = new ExpressionPropertyAccessor(property);
                _cache.AddOrUpdate(type, new ConcurrentBag<ExpressionPropertyAccessor> {accessor}, (key, value) =>
                {
                    value.Add(accessor);
                    return value;
                });
            });

            // inject into fields
            type.FieldsWithAttribute<AutowireAttribute>()
                .ForEach(field =>
                {
                    var accessor = new ExpressionFieldAccessor(field);

                    _fieldCache.AddOrUpdate(type, new ConcurrentBag<ExpressionFieldAccessor> {accessor}, (key, value) =>
                    {
                        value.Add(accessor);
                        return value;
                    });
                });
        }
        public TRType Inject<TRType>(TRType objectToInjectTo)
        {
            Type type = objectToInjectTo.GetType();

            if (objectToInjectTo == null) throw new ArgumentNullException(nameof(objectToInjectTo));

            if (!_cache.ContainsKey(type))
            {
                CacheType(type);
            }
            if (_cache.ContainsKey(type))
                _cache[type].ForEach(o => o.SetValue(objectToInjectTo,Services.GetService(o.PropertyType)));
            if (_fieldCache.ContainsKey(type))
                _fieldCache[type].ForEach(o => o.SetValue(objectToInjectTo, Services.GetService(o.FieldType)));

            return objectToInjectTo;
        }

    }
}
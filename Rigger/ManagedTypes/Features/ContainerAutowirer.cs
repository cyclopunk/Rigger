using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rigger.Extensions;
using Rigger.Exceptions;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.Reflection;

namespace Rigger.ManagedTypes.Features
{
    /// <summary>
    /// Class that will preform autowiring by iterating through fields and properties and finding the [Autowire] attribute
    /// </summary>
    public class ContainerAutowirer : IAutowirer, IServiceAware
    {
        private static IDictionary<Type, List<ExpressionPropertyAccessor>> _cache =
            new ConcurrentDictionary<Type, List<ExpressionPropertyAccessor>>(); 
        private static IDictionary<Type, List<ExpressionFieldAccessor>> _fieldCache =
            new ConcurrentDictionary<Type, List<ExpressionFieldAccessor>>();
        public IServices Services { get; set; }

        public void CacheType(Type type)
        {
            var propList = _cache.GetOrPut(type, () => new List<ExpressionPropertyAccessor>());
            var fieldList = _fieldCache.GetOrPut(type, () => new List<ExpressionFieldAccessor>());
            type.PropertyWithAttribute<AutowireAttribute>().ForEach(property =>
            {
                propList.Add(new ExpressionPropertyAccessor(property));
            });

            // inject into fields
            type.FieldsWithAttribute<AutowireAttribute>()
                .ForEach(field =>
                {
                    var list = _fieldCache.GetOrPut(type, () => new List<ExpressionFieldAccessor>());
                    fieldList.Add(new ExpressionFieldAccessor(field));
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

            _cache[type].ForEach(o => o.SetValue(objectToInjectTo,Services.GetService(o.PropertyType)));
            _fieldCache[type].ForEach(o => o.SetValue(objectToInjectTo, Services.GetService(o.FieldType)));

            return objectToInjectTo;
        }

    }
}
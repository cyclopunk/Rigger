using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rigger.Extensions
{
    public class AttributeReflections
    {
        public Type Attribute { get;  }
        public Type Type { get;  }

        internal AttributeReflections(Type type, Type attr)
        {
            this.Attribute = attr;
            this.Type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeReflections rm)
            {
                return rm.Type == Type && rm.Attribute == Attribute;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Attribute, Type);
        }
    }
    /// <summary>
    /// Extension methods to help with reflection tasks. At some point I would like to back this
    /// with a reflection cache to make things easier.
    /// </summary>
    public static class ReflectionExtensions
    {
        private static ConcurrentDictionary<Type, IEnumerable<MethodInfo>> _typeAttributeCache =
            new ConcurrentDictionary<Type, IEnumerable<MethodInfo>>();
        private static ConcurrentDictionary<AttributeReflections, IEnumerable<MethodInfo>> _attributeMethodCache =
            new ConcurrentDictionary<AttributeReflections, IEnumerable<MethodInfo>>();
        private static ConcurrentDictionary<AttributeReflections, IEnumerable<FieldInfo>> _attributeFieldCache =
            new ConcurrentDictionary<AttributeReflections, IEnumerable<FieldInfo>>();
        
        private static ConcurrentDictionary<AttributeReflections, IEnumerable<PropertyInfo>> _attributePropertyCache =
            new ConcurrentDictionary<AttributeReflections, IEnumerable<PropertyInfo>>();

        // Default Binding Flags 
        public static BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static; 
        /// <summary>
        /// A functor that produces a list (i.e. makes a enumeration concrete) after applying
        /// a function to it.
        /// </summary>
        /// <typeparam name="TInType"></typeparam>
        /// <typeparam name="TOutType"></typeparam>
        /// <param name="enumerable">The enumeration to map</param>
        /// <param name="func">The mapping function</param>
        /// <returns></returns>
        public static List<TOutType> Map<TInType,TOutType>(this IEnumerable<TInType> enumerable, Func<TInType, TOutType> func)
        {
            return enumerable.Select(func)
                    .ToList();
        }
        /// <summary>
        /// Get all types in an assembly that contain an attribute.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> TypesWithAttributes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes
                    .Where(o => o.GetCustomAttributes(true).Length > 0);
            }
            catch (Exception)
            {
                // if any exception happens, just return a blank list.
                return new List<Type>();
            }
        }
        /// <summary>
        /// Get all types in an assembly that extend an interface.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> TypesWithInterface<TInterface>(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes
                    .Where(o => o.GetInterfaces().Contains(typeof(TInterface)));
            }
            catch (Exception)
            {
                // if any exception happens, just return a blank list.
                return new List<Type>();
            }
        }

        public static IEnumerable<MethodInfo> MethodsWithAttribute(this Type type, Type attributeType)
        {
            var cached = _attributeMethodCache.GetOrAdd(new AttributeReflections(type,attributeType) , (k) =>
            {
                try
                {
                    return type.GetMethods(defaultBindingFlags)
                        .Where(m => m.GetCustomAttribute(attributeType) != null);
                }
                catch (Exception)
                {
                    // if any exception happens, just return a blank list.
                    return new List<MethodInfo>();
                }
            });

            return cached;
        }
        public static IEnumerable<MethodInfo> MethodsWithAttribute<TAttribute>(this Type type) where TAttribute:Attribute
        {
            return MethodsWithAttribute(type, typeof(TAttribute));
        }

        public static IEnumerable<FieldInfo> FieldsWithAttribute(this Type type, Type attributeType)
        {
            var cached = _attributeFieldCache.GetOrAdd(new AttributeReflections(type,attributeType) , (k) =>
            {
                try
                {
                    return type.GetFields(defaultBindingFlags)
                        .Where(m => m.GetCustomAttribute(attributeType) != null);
                }
                catch (Exception)
                {
                    // if any exception happens, just return a blank list.
                    return new List<FieldInfo>();
                }
            });

            return cached;
        }
        public static IEnumerable<FieldInfo> FieldsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return FieldsWithAttribute(type, typeof(TAttribute));
        }
        public static IEnumerable<FieldInfo> FieldsNamed(this Type type, string name)
        {
            return type.GetFields(defaultBindingFlags)
                .Where(m => m.Name == name);
        }
        public static IEnumerable<MethodInfo> MethodsNamed(this Type type, string name)
        {
            return type.GetMethods(defaultBindingFlags)
                .Where(m => m.Name == name);
        }
        public static IEnumerable<PropertyInfo> PropertiesWithAttribute(this Type type, Type attributeType)
        {
            var cached = _attributePropertyCache.GetOrAdd(new AttributeReflections(type,attributeType) , (k) =>
            {
                try
                {
                    return type.GetProperties(defaultBindingFlags)
                        .Where(m => m.GetCustomAttribute(attributeType) != null);
                }
                catch (Exception)
                {
                    // if any exception happens, just return a blank list.
                    return new List<PropertyInfo>();
                }
            });

            return cached;
        }
        public static IEnumerable<PropertyInfo> PropertiesWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return PropertiesWithAttribute(type, typeof(TAttribute));
        }
        public static IEnumerable<Type> TypesWithAttribute<TAttr>(this Assembly assembly)
        {
            try
            {
                var definedTypes = assembly.DefinedTypes
                    .Where(o => o.GetCustomAttribute(typeof(TAttr)) != null);

                return definedTypes;
            }
            catch (Exception)
            {
                // if any exception happens, just return a blank list.
                return new List<Type>();
            }
        }

        public static IEnumerable<PropertyInfo> PropertiesOfType(this Type type, Type typeToFind) 
        {
            return type.GetProperties(defaultBindingFlags).Where(o => o.PropertyType == typeToFind).ToList();
        }
        public static IEnumerable<FieldInfo> FieldsOfType(this Type type, Type typeToFind)
        {
            return type.GetFields(defaultBindingFlags).Where(o => o.FieldType == typeToFind).ToList();
        }
        public static T GetInstance<T>(this Type type)
        {
           return (T) Activator.CreateInstance(type);
        }
        /**
         * Get a copy by using FastDeepCloner.
         */
        public static T GetCopy<T>(this object o)
        {
            return (T) FastDeepCloner.DeepCloner.Clone(o);
        }

        public static object FieldValue(this object obj, string fieldName)
        {
            var type = obj.GetType();
            return type.GetField(fieldName)
                .GetValue(obj);
        }
        public static MethodInfo MethodNamed(this object obj, string name, params Type[] arguments)
        {
            var type = obj is Type isType ? isType : obj.GetType();
            return arguments.Length == 0 ? type.GetMethod(name, defaultBindingFlags) : type.GetMethod(name, arguments);
        }
    
        /// <summary>
        /// Explode properties into an object array. 
        /// </summary>
        /// <param name="objectToExplode"></param>
        /// <returns></returns>
        public static object[] ExplodeProperties(this object objectToExplode)
        {
            return objectToExplode.GetType().GetProperties()
                .Select(p => p.GetValue(objectToExplode))
                .ToArray();
        }
    

        public static bool HasTypeAttribute(this Type type, Type attr)
        {
            return type.GetCustomAttribute(attr, true) != null;
        }
    }
}
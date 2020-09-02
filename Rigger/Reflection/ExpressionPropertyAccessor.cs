using System;
using System.Reflection;
using Rigger.Extensions;

namespace Rigger.Reflection
{
    /// <summary>
    /// PropertyInvoker that will use expressions to access properties using reflection.
    /// </summary>
    public class ExpressionPropertyAccessor : IPropertyInvoker, IValueGetter
    {
        public bool IsStatic { get; private set; }
        public PropertyInfo Property { get; set; }

        private ZeroParameterMethodAccessor getMethodAccessor;
        private SingleParameterMethodAccessor setMethodAccessor;
        public ExpressionPropertyAccessor(Type t, String propertyName) : this(t.GetProperty(propertyName, ReflectionExtensions.defaultBindingFlags))
        {
            PropertyName = propertyName;
        }

        public ExpressionPropertyAccessor(PropertyInfo info)
        {
            this.Property = info;
            
            PropertyType = info.PropertyType;

            IsStatic = info.SetMethod.IsStatic;

            getMethodAccessor = new ZeroParameterMethodAccessor(info.GetMethod);
            setMethodAccessor = new SingleParameterMethodAccessor(info.SetMethod);
        }

        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }

        public void SetValue(object source, object o)
        {
            setMethodAccessor.Invoke(IsStatic ? null : source, o);
        }

        public object GetValue(object source)
        {
            return getMethodAccessor.Invoke(IsStatic ? null : source);
        }
    }
}
using System;

namespace Rigger.Reflection
{
    /// <summary>
    /// Interface for accessing properties of an object via reflection. 
    /// </summary>
    public interface IPropertyInvoker
    {
        string PropertyName { get; set; }
        Type PropertyType { get; set; }
        void SetValue(object dest, object value);
        object GetValue(object dest);

    }
    public interface IPropertyInvoker<TClass, TFieldType>
    {
        string PropertyName { get; set; }
        void SetValue(TClass dest, TFieldType value);
        object GetValue(TClass dest);

    }
}
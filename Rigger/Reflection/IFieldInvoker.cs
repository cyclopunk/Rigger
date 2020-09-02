using System;

namespace Rigger.Reflection
{
    /// <summary>
    /// Interface for a field invoker. Field invokers will set and get values from fields
    /// using reflection.
    /// </summary>
    public interface IFieldInvoker
    {
        string FieldName { get; set; }
        Type FieldType { get; set; }
        void SetValue(object dest, object value);
        object GetValue(object dest);
    }

    /// <summary>
    /// Type specific field invoker.
    /// </summary>
    /// <typeparam name="TClass">The class that this field resides on</typeparam>
    /// <typeparam name="TFType">The type that this field returns.</typeparam>
    public interface IFieldInvoker<TClass, TFType>
    {
        string FieldName { get; set; }
        void SetValue(TClass dest, TFType value);
        object GetValue(TClass dest);
    }
}
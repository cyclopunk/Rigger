using System;
using System.Linq.Expressions;
using System.Reflection;
using Rigger.Extensions;

namespace Rigger.Reflection
{
    /// <summary>
    /// Class that uses expressions to extend access to a field.
    /// </summary>
    public class ExpressionFieldAccessor : IFieldInvoker, IValueGetter
    {
        public bool IsStatic { get; private set; }
        private readonly Action<object, object> fieldSetter;
        private readonly Func<object, object> fieldGetter;
        public object GetValue(object source)
        {
            return fieldGetter.Invoke(IsStatic ? null : source);
        }

        public void SetValue(object source, object value)
        {
            fieldSetter.Invoke(IsStatic ? null : source, value);
        }

        public ExpressionFieldAccessor(Type t, string fieldName) : this(
            t.GetField(fieldName, ReflectionExtensions.defaultBindingFlags) ?? throw new ArgumentException($"{t} does not have a field named {fieldName}"))
        {
            FieldName = fieldName;
        }
        public ExpressionFieldAccessor(FieldInfo fieldInfo)
        {
            FieldType = fieldInfo.FieldType;
            IsStatic = fieldInfo.IsStatic;
            fieldGetter = GetGetFieldDelegate(fieldInfo);
            fieldSetter = GetSetFieldDelegate(fieldInfo);
        }
        public Func<object, object> GetGetFieldDelegate(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            Type fieldDeclaringType = fieldInfo.DeclaringType;

            ParameterExpression sourceParameter =
                Expression.Parameter(typeof(object), "source");


            Expression sourceExpression =  GetCastOrConvertExpression(
                sourceParameter, fieldDeclaringType);

            MemberExpression fieldExpression = Expression.Field(IsStatic ? null : sourceExpression, fieldInfo);

            Expression resultExpression = this.GetCastOrConvertExpression(
                fieldExpression, typeof(object));

            LambdaExpression lambda =
                Expression.Lambda<Func<object,object>>(resultExpression, sourceParameter);

            return Expression.Lambda<Func<object, object>>(resultExpression, sourceParameter).Compile();
        }

        public Action<object,object> GetSetFieldDelegate(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");

            Type fieldDeclaringType = fieldInfo.DeclaringType;
            
            ParameterExpression sourceParameter = Expression.Parameter(typeof(object), "source");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            Expression sourceExpression = this.GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);

            // Get the field access expression.
            Expression fieldExpression = Expression.Field(IsStatic ? null : sourceExpression, fieldInfo);

            // Add cast or convert expression if necessary.
            Expression valueExpression = this.GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

            // Get the generic method that assigns the field value.
            var genericSetFieldMethodInfo = setFieldMethod.MakeGenericMethod(fieldExpression.Type);

            // get the set field expression 
            // e.g. source.SetField(ref (arg as MyClass).integerProperty, Convert(value)
            MethodCallExpression setFieldMethodCallExpression = Expression.Call(
                null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

            return Expression.Lambda<Action<object, object>>(
                setFieldMethodCallExpression, sourceParameter, valueParameter).Compile();
        }
        private Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {
            Expression result;
            Type expressionType = expression.Type;

            if (targetType.IsAssignableFrom(expressionType))
            {
                result = expression;
            }
            else
            {
                if (targetType.IsValueType && !IsNullableType(targetType))
                {
                    result = Expression.Convert(expression, targetType);
                }
                else
                {
                    result = Expression.TypeAs(expression, targetType);
                }
            }

            return result;
        }
        private static readonly MethodInfo setFieldMethod =
            typeof(ExpressionFieldAccessor).GetMethod("SetField",
                BindingFlags.NonPublic | BindingFlags.Static);

        private static void SetField<TValue>(ref TValue field, TValue newValue)
        {
            field = newValue;
        }
        private bool IsNullableType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            bool result = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            return result;
        }

        public string FieldName { get; set; }
        public Type FieldType { get; set; }
    }
}
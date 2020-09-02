using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rigger.Reflection
{
    public class ZeroParameterMethodAccessor
    {
        private Type type;
        private MethodInfo info;
        private readonly Func<object, object> callMethodAccessor;
        public ZeroParameterMethodAccessor(Type type, string methodName)
        {
            this.type = type;
            callMethodAccessor = GenerateMethodAccessor<object>(type.GetMethod(methodName) ?? throw new ArgumentException($"Could not find method {methodName} on {type}"));
        }
        public ZeroParameterMethodAccessor(MethodInfo methodInfo)
        {
            this.type = methodInfo.DeclaringType ?? throw new NullReferenceException("MethodInfo declaring type is null, cannot use a method accessor on anonymous methods.");
            callMethodAccessor = GenerateMethodAccessor<object>(methodInfo ?? throw new ArgumentNullException($"MethodInfo is null"));
        }

        private Func<T, object> GenerateMethodAccessor<T>(MethodInfo methodInfo)
        {
            this.info = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            // instance method is called from
            ParameterExpression typeParameter = Expression.Parameter(typeof(object), "method");

            MethodCallExpression methodCall = Expression.Call( methodInfo.IsStatic ? null :
                    Expression.ConvertChecked(typeParameter, type),
                methodInfo);

            return Expression.Lambda<Func<T, object>>(Expression.Convert(methodCall, typeof(object)), typeParameter).Compile();
        }

        public object Invoke(object source)
        {
            return callMethodAccessor(source);
        }
    }
}
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
        private readonly  Action<object> voidMethodAccessor;
        public ZeroParameterMethodAccessor(Type type, string methodName)
        {
            this.type = type;
            callMethodAccessor = GenerateMethodAccessor<object>(type.GetMethod(methodName) ?? throw new ArgumentException($"Could not find method {methodName} on {type}"));
        }
        public ZeroParameterMethodAccessor(MethodInfo methodInfo)
        {
            this.type = methodInfo.DeclaringType ?? throw new NullReferenceException("MethodInfo declaring type is null, cannot use a method accessor on anonymous methods.");
            if (methodInfo.ReturnType == typeof(void))
            {
                voidMethodAccessor = GenerateVoidMethodAccessor<object>(methodInfo ??
                                                                        throw new ArgumentNullException($"MethodInfo is null"));
            }
            else
            {
                callMethodAccessor =
                    GenerateMethodAccessor<object>(methodInfo);
            }
        }
        private Action<object> GenerateVoidMethodAccessor<T>(MethodInfo methodInfo)
        {
            this.info = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            // instance method is called from
            ParameterExpression typeParameter = Expression.Parameter(typeof(object), "method");

            MethodCallExpression methodCall = Expression.Call( methodInfo.IsStatic ? null :
                    Expression.ConvertChecked(typeParameter, type),
                methodInfo);

            return Expression.Lambda<Action<object>>(methodCall, typeParameter)
                .Compile();
           
        }
        private Func<T, object> GenerateMethodAccessor<T>(MethodInfo methodInfo)
        {
            this.info = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            // instance method is called from
            ParameterExpression typeParameter = Expression.Parameter(typeof(object), "method");

            MethodCallExpression methodCall = Expression.Call( methodInfo.IsStatic ? null :
                    Expression.ConvertChecked(typeParameter, type),
                methodInfo);

            return Expression.Lambda<Func<T, object>>(Expression.Convert(methodCall, typeof(object)), typeParameter)
                .Compile();
        }

        /// <summary>
        /// Invokes the expression. Note that if the method has a void return type this method will return the bool value true.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public object Invoke(object source)
        {
            // this is funky, probably not what I want
            if (voidMethodAccessor != null)
            {
                voidMethodAccessor(source);

                return true;
            }
            return callMethodAccessor(source);
            
        }
    }
}
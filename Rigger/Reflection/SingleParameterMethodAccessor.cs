using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rigger.Reflection
{
    /// <summary>
    /// Method accessor using Expressions that allows for one argument.
    /// </summary>
    public class SingleParameterMethodAccessor
    {
        private readonly Type type;
        private readonly Action<object, object> callMethodAccessor;

        public SingleParameterMethodAccessor(Type type, string methodName) : this(type.GetMethod(methodName))
        {

        }

        public SingleParameterMethodAccessor(MethodInfo info)
        {
            if (info == null)
                throw new ArgumentNullException($"Methodinfo is null for parametermethodaccessor on {type}");
            this.type = info.DeclaringType;

            callMethodAccessor = GenerateMethodAccessor(info);
        }

        private Action<object, object> GenerateMethodAccessor(MethodInfo methodInfo)
        {
            var firstParameterType = methodInfo.GetParameters().First().ParameterType;

            ParameterExpression returnParameter = Expression.Parameter(typeof(object), "method");
            ParameterExpression valueArgument = Expression.Parameter(typeof(object), "argument");

            MethodCallExpression setterCall = Expression.Call(methodInfo.IsStatic ? null :
                Expression.ConvertChecked(returnParameter, type),
                methodInfo,
                Expression.Convert(valueArgument, firstParameterType));

            return Expression.Lambda<Action<object, object>>(setterCall, returnParameter, valueArgument).Compile();
        }

        public void Invoke(object source, object parameter)
        {
            callMethodAccessor(source, parameter);
        }
    }
}
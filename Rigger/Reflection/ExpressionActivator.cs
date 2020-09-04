using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rigger.Extensions;

namespace Rigger.Reflection
{
    // interface for a typesafe activator
    public interface IActivator<T>
    {
        T Activate(params object[] args);
    }
   
    /// <summary>
    /// Class that will use expressions to construct an instance from
    /// the passed in type.
    /// </summary>
    public class ExpressionActivator : ExpressionActivator<object>
    {
        /// <summary>
        /// Construct an expression activator for the provided type.
        /// </summary>
        /// <param name="type"></param>
        public ExpressionActivator(Type type) : base(type)
        {

        }
        public ExpressionActivator(ConstructorInfo info) : base(info)
        {

        }
    }
    /// <summary>
    /// Activator that uses an expression to create a instance of a class
    /// from a type provided in the constructor.
    /// </summary>
    public class ExpressionActivator <TInstance> : IActivator<TInstance>
    {
        private static readonly ConcurrentDictionary<ConstructorInfo, ObjectActivator<TInstance>> cache =
            new ConcurrentDictionary<ConstructorInfo, ExpressionActivator<TInstance>.ObjectActivator<TInstance>>();
        private ObjectActivator<TInstance> _activator;


        public ExpressionActivator() : this(typeof(TInstance))
        {
        }
        public ExpressionActivator(Type type) : this(type.GetConstructors().First())
        {
        }
        public ExpressionActivator(ConstructorInfo info)
        {
            _activator = cache.GetOrAdd(info, GetActivator<TInstance>);
        }

        public TInstance Activate(params object[] args)
        {
            return _activator(args);
        }

        public delegate T ObjectActivator<T>(params object[] args);
        public ObjectActivator<T> GetActivator<T>
            (ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            NewExpression newExp = Expression.New(ctor, argsExp);

            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();

            return compiled;
        }
    }
}
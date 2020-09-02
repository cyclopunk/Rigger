using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheCommons.Core.Extensions;
using TheCommons.Core.Reflection;
using TheCommons.Forge.Exceptions;
using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes.Implementations
{
    /// <summary>
    ///  This method invoker will resolve types if there for support of service resolution of parameters.
    ///
    ///  this will allow methods such as:
    ///  [OnEvent(Event=typeof(SomeCoolEvent))]
    ///  void DoSomething(ILogger logger, SomeCoolEvent evt){
    ///     logger.Debug("Got an event: {evt}");
    ///  }
    /// where logger would be looked up in the container.
    /// </summary>
    public class ManagedMethodInvoker : IMethodInvoker
    {
        [Autowire] private IContainer _container;
        public string MethodName { get; set; }

        private MethodInfo _info;


        public ManagedMethodInvoker(Type type, string methodName)
        {
            MethodName = methodName;
            _info = type.MethodNamed(methodName)
                    ?? throw new ArgumentException($"Method {methodName} cannot be found on {type}");
        }

        public ManagedMethodInvoker(MethodInfo method)
        {
            _info = method;
        }

        /// <summary>
        /// Invoke a method. All parameters will attempt to be resolved,
        /// the values passed in will be appended to the resolved parameters (pass in none to use entirely resolved parameters)
        ///
        /// TODO make this smarter to allow parameters to be resolved out of order
        /// </summary>
        /// <param name="values"></param>
        public object Invoke(object dest, params object[] values)
        {
            // parameter resolution via container lookup

            if (_container == null)
            {
                throw new ContainerException($"Cannot autowire {MethodName} for {dest} because the invoker lacks a container.");
            }

            Type[] genericArguments = _info.GetGenericArguments();

            if (genericArguments.Length > 0 && _info.IsGenericMethodDefinition)
            {

                var types = genericArguments
                    .Select(arg => _container.Get(arg.BaseType).GetType())
                    .ToArray();
                
                _info = _info.MakeGenericMethod(types);
            }

            var invokedTypes = values.Map(o => o.GetType());

            var paramsList = _info.GetParameters()
                .FindAll(mi=> !invokedTypes.Contains(mi.ParameterType)) // don't fill in the parameters from the types passed in values
                .Map(m =>
                {
                    try
                    { 
                        // parameter type may be an inherited type, so try the base type as a lookup as well.
                        return _container.Get(m.ParameterType) ?? _container.Get(m.ParameterType.BaseType);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });

            IEnumerable<object> allParameters = paramsList?.FindAll(o => o != null).Concat(values);

            return _info.Invoke(dest, allParameters?.ToArray() ?? new object[] {});
        }

        private List<object> ResolveParameters()
        {
            return _info.GetParameters().Map(m => _container.Get(m.GetType()));
        }
    }
}
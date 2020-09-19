using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Rigger.Configuration;
using Rigger.Extensions;
using Rigger.Reflection;
using Rigger.Attributes;
using Rigger.Injection;

namespace Rigger.ManagedTypes.Implementations
{

    public class ScopedConstructorInvoker : IConstructorActivator
    {
        private Guid scopeId;

        private ManagedConstructorInvoker invoker;

        public ScopedConstructorInvoker(Guid scopeId, ManagedConstructorInvoker invoker)
        {
            this.invoker = invoker;
            this.scopeId = scopeId;
        }

        public object Construct(params object[] parameters)
        {
            return invoker.Construct(new object[] { scopeId }.Concat(parameters));
        }
        public object Construct(Type type, params object[] parameters)
        {
            return invoker.Construct(new object[] { scopeId }.Concat(parameters));
        }
    }

    /// <summary>
    /// A constructor invoker that takes in a container to use for service
    /// lookup. Meant to be cached.
    /// </summary>
    public class ManagedConstructorInvoker : IConstructorActivator, IServiceAware
    {
        
        private static ConcurrentDictionary<Type, ConstructorInfo> ctorCache =
            new ConcurrentDictionary<Type, ConstructorInfo>();

        private readonly Type _typeToConstruct;

        public IServices Services { get; set; }

        public ManagedConstructorInvoker()
        {

        }

        public object Construct<T>(params object[] parameters)
        {   
            return this.Construct(typeof(T), parameters);
        }

        /// <summary>
        /// Backwards compatability.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Construct(params object[] parameters)
        {
            return this.Construct(_typeToConstruct, parameters);
        }
        /// <summary>
        /// Construct an object with the parameters. This method will find an autowirable constructor and
        /// also use the container to inject.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Construct(Type type, params object[] parameters)
        {

            // ILogger<ManagedConstructorInvoker> log = Services.GetService<ILogger<ManagedConstructorInvoker>>();

           // log?.LogInformation($"ManagedConstructorInvoker: Constructing {_typeToConstruct.Name} parameters: {string.Join(',',parameters)}");
            
       
            var constructedTypes = parameters.Select(o => o.GetType());

            var autowireConstructor = type
                .GetConstructors().FirstOrDefault(o => CustomAttributeExtensions.GetCustomAttribute<AutowireAttribute>((MemberInfo) o) != null);
            
            List<ConstructorInfo> constructors = new List<ConstructorInfo>();

            if (autowireConstructor == null)
            {
                // Get all the constructors
                constructors = type
                    .GetConstructors(ReflectionExtensions.defaultBindingFlags)
                    .Where(o => !o.IsStatic && o.GetCustomAttribute<ObsoleteAttribute>() == null)
                    .ToList();
            } else constructors.Add(autowireConstructor);

            IEnumerable<object> allParameters = null;

            if (constructors.Count > 1)
            {
             //   log.LogInformation("Multiple constructors to choose from: " + constructors);
            }

            
               // log.LogInformation("Trying to construct " + constructor);
            try
            {
                var constructor = ctorCache.GetOrAdd(type, (t) => FindBestConstructor(constructors));

                var paramsList = constructor
                    .GetParameters()
                    .Where(mi =>
                        !constructedTypes.Contains(mi
                            .ParameterType)) // don't fill in the parameters from the types passed in values
                    .Select(m =>
                    {
                        ValueAttribute v = m.GetCustomAttribute<ValueAttribute>();
                        // if it's a value, get it from the configuration service, if it's not, use the container to look it up.
                        if (v != null && Services.IsManaged<IConfigurationService>()) return Services.GetService<IConfigurationService>().Get(v.Key);

                        var lookedUpObject = Services.GetService(m.ParameterType, CallSiteType.Constructor);

                        return lookedUpObject;

                    });

                allParameters = paramsList?.Concat(parameters).ToList();

                if (allParameters.Count() != constructor.GetParameters()?.Length)
                {
                    //log.LogError("ManagedConstructorInvoker: Param count doesn't match.");

                    return null;
                }

                return
                    new ExpressionActivator(constructor)
                        .Activate(allParameters?.ToArray()); 

            }
            catch (Exception e)
            {
                Debug.WriteLine(e,$"Could not instantiate using constructor {type.Name}({constructors}");
            }

            return null;
        }

        private ConstructorInfo FindBestConstructor(IEnumerable<ConstructorInfo> constructors)
        {
            var bestConstructor = constructors
                .OrderByDescending(CountResolveableParamaters)
                .First();

            return bestConstructor;
        }

        private int CountResolveableParamaters(ConstructorInfo info)
        {
            return info.GetParameters().ToList()
                .Count(o =>
                {
                    if (o.ParameterType.IsGenericType && o.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        return Services.IsManaged(o.ParameterType.GetGenericArguments().First());
                    }
                    return Services.IsManaged(o.ParameterType);
                });
        }
    }
}
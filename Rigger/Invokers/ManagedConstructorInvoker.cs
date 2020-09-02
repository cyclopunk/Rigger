﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Rigger.Configuration;
using Rigger.Extensions;
using Rigger.Reflection;
using Rigger.ManagedTypes.Lightweight;
using Rigger.ManagedTypes.ServiceLocator;
using Rigger.Attributes;

namespace Rigger.ManagedTypes.Implementations
{

    public class ScopedConstructorInvoker : IConstructorInvoker
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
    public class ManagedConstructorInvoker : IConstructorInvoker, IServiceAware
    {
        
        private static IDictionary<Type, ConstructorInfo> ctorCache =
            new Dictionary<Type, ConstructorInfo>();
        private readonly IContainer _container;
        private readonly Type _typeToConstruct;

        public Services Services { get; set; }

        public ManagedConstructorInvoker()
        {

        }
        public ManagedConstructorInvoker(IContainer container, Type typeToConstruct)
        {
            this._container = container;
            this._typeToConstruct = typeToConstruct;
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
            Guid scopeId = Guid.Empty;

            if (parameters.FirstOrDefault() != null && parameters.First() is Guid scope)
            {
                scopeId = scope;
                parameters = parameters.TakeLast(parameters.Length - 1).ToArray();
            }

            ILogger log = Services.GetService<ILogger>();

           // log?.LogInformation($"ManagedConstructorInvoker: Constructing {_typeToConstruct.Name} parameters: {string.Join(',',parameters)}");
            
       
            var constructedTypes = parameters.Map(o => o.GetType());

            var autowireConstructor = type.GetConstructors()
                .Find(o => o.GetCustomAttribute<AutowireAttribute>() != null);
            
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
                var constructor = ctorCache.GetOrPut(type, () => FindBestConstructor(constructors));

                var paramsList = constructor
                    .GetParameters()
                    .FindAll(mi =>
                        !constructedTypes.Contains(mi
                            .ParameterType)) // don't fill in the parameters from the types passed in values
                    .Map(m =>
                    {
                        ValueAttribute v = m.GetCustomAttribute<ValueAttribute>();
                        // if it's a value, get it from the configuration service, if it's not, use the container to look it up.
                        if (v != null) return Services.GetService<IConfigurationService>().Get(v.Key);

                        var lookedUpObject = Services.GetService(m.ParameterType);

                        return lookedUpObject;

                    });

                allParameters = paramsList?.Concat(parameters).ToList();

                if (allParameters.Count() != constructor.GetParameters()?.Length)
                {
                    log.LogError("ManagedConstructorInvoker: Param count doesn't match.");

                    return null;
                }

                return
                    new ExpressionActivator(constructor)
                        .Activate(allParameters?.ToArray()); 

            }
            catch (Exception e)
            {
                log.LogError(e,$"Could not instantiate using constructor {type.Name}({constructors}");
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
                        return Services.Get(o.ParameterType.GetGenericArguments().First()) != null;
                    }
                    return Services.Get(o.ParameterType) != null;
                });
        }
    }
}
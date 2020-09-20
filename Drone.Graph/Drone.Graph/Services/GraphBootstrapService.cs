using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Drone.Graph.Proxy;
using EntityGraphQL.Authorization;
using EntityGraphQL.Schema;
using Microsoft.EntityFrameworkCore;
using Rigger.Abstractions;
using Rigger.API.Attributes;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.Implementations;
using Rigger.Reflection;

namespace Drone.Graph.Services
{

        [Bootstrap]
        [RequiresService(typeof(DbContext))]
        class GraphBootstrapService
        {
            [Autowire] private IContainer container;

            /// <summary>
            /// Startup method that will run when the application container is in the
            /// bootstrap phase.
            /// </summary>
            /// <typeparam name="TContextType"></typeparam>
            /// <param name="context"></param>
            [OnStartup]
            void GraphQLStart<TContextType>(TContextType context)
                where TContextType : DbContext // will look up the type by DbContext
            {
                if (context == null)
                {
                    throw new Exception("DbContext was not registered, cannot Bootstrap GraphQL.");
                }

                var provider = SchemaBuilder.FromObject<TContextType>();
                
                provider.AddCustomScalarType(typeof(DateTime?), "Date");
                provider.AddCustomScalarType(typeof(DateTime), "Date");
                provider.AddCustomScalarType(typeof(DateTimeOffset), "DateOffset");
                provider.AddCustomScalarType(typeof(DateTimeOffset?), "DateTimeOffset");
                var services = container.Services.List().ToList();

                /*
                 * Find all custom scalars 
                 */
                container.Services.List().Where(mt => mt.ImplementationType.GetCustomAttribute<ScalarAttribute>() != null)
                    .Select(mtype => mtype.ImplementationType)
                    .ForEach(scalar =>
                    {
                        SchemaInformationAttribute info = scalar.GetCustomAttribute<SchemaInformationAttribute>();

                        provider.AddCustomScalarType(scalar, info?.Name ?? scalar.Name);
                    });

                /*
                 * Find all custom types marked with the External attribute.
                 */
                services.Where(mt => mt.ImplementationType.GetCustomAttribute<ExternalAttribute>() != null)
                    .Select(mtype => mtype.ImplementationType).ForEach(type =>
                    {
                        SchemaInformationAttribute info = type.GetCustomAttribute<SchemaInformationAttribute>();

                        provider.AddType(type, info?.Name ?? type.Name,
                                info?.Description ?? $"No description defined for {type.Name}")
                            .AddAllFields(true);
                    });

                services.Where(mt => mt.ImplementationType.GetCustomAttribute<MutatorAttribute>() != null)
                    .Select(mtype => container.Get(mtype.ImplementationType))
                    .ForEach(mutator => { provider.AddMutationFrom(mutator); });

                // scan all singletons for Query methods
                // these will the be added to the MappedSchemaProvider

                services.Where(mt => mt.ImplementationType.GetCustomAttribute<SingletonAttribute>() != null)
                    .Select(mtype => mtype.ImplementationType)
                    .Select(t => t.MethodsWithAttribute<QueryAttribute>())
                    .Combine()
                    .ForEach(managedTypeQueryMethod =>
                    {
                        var declaringType = managedTypeQueryMethod.DeclaringType;

                        // get the schema information, if it exists.

                        var info = managedTypeQueryMethod.GetCustomAttribute<SchemaInformationAttribute>();
                        var authorize = managedTypeQueryMethod
                            .GetCustomAttribute<AuthorizeAttribute>();

                        RequiredClaims claims = null;

                        if (authorize != null)
                        {
                            // Transforms generic Authorize attribute to the GraphQL Attribute that EntityGraphQL expects.  
                            claims = new RequiredClaims(new List<GraphQLAuthorizeAttribute>
                            {
                                new GraphQLAuthorizeAttribute(authorize.Claims.ToArray())
                            });
                        }

                        var invoker = new ManagedMethodInvoker(declaringType, managedTypeQueryMethod.Name).AddServices(container.Services);

                        // Build a dynamic object that will have the properties
                        // of the method's parameters. This will be used to describe the 
                        // parameters that the query takes.

                        var parameterTypeInstance = BuildParameterInstance(managedTypeQueryMethod);

                        /*
                         * Build a lambda expression from the method, the parameter types and the method invoker.
                         */
                        var lambdaExpression = MakeLambdaExpressionForMethod(managedTypeQueryMethod,
                            parameterTypeInstance.GetType(), invoker);

                        /*
                         * Create the EntityGraphQL field which will be added to the schema provider.
                         *
                         */
                        // new RequiredClaims(new List<GraphQLAuthorizeAttribute>{new GraphQLAuthorizeAttribute(new string[] {"test"})});
                        var f = new Field(info?.Name ?? managedTypeQueryMethod.Name,
                            lambdaExpression,
                            info?.Description ?? "No Description",
                            managedTypeQueryMethod.ReturnType.Name, parameterTypeInstance, null);

                        // add the field to the schema provider.
                        provider.AddField(f);
                    });
                container.Register<ISchemaProvider>(provider);

                provider.AddType<DbContextId>("DbContextId");
            }

            /// <summary>
            /// EntityGraphQL requires an anonymous object that has properties to build the
            /// parameters that queries can take. This builds a dynamic instance with those
            /// parameters.
            /// </summary>
            /// <returns></returns>
            private object BuildParameterInstance(MethodInfo method)
            {
                var tFoundry = new Foundry.TypeFoundry(method.Name + "Impl");

                method.GetParameters()
                    .ForEach(p =>
                        tFoundry.AddProperty(p.Name, p.ParameterType));

                return Activator.CreateInstance(tFoundry.Build());
            }

            /// <summary>
            /// Build a LambdaExpression of the form: 
            ///    (ctx, params) => invoker.Invoke(container.Get(ServiceMethodReturnType), params)
            ///      |- The Db Context
            ///            ^
            ///            | The Params Object
            ///                       ^
            ///                       | Invoke the method provided.
            /// </summary>
            /// <param name="method">The MethodInfo for the method on the ManagedType that will run this query</param>
            /// <param name="paramsType">The "param" object type.</param>
            /// <param name="serviceMethodInvoker">The method invoker that will call the ManagedType method</param>
            /// <returns></returns>
            private LambdaExpression MakeLambdaExpressionForMethod(MethodInfo method, Type paramsType,
                IMethodInvoker serviceMethodInvoker)
            {

                // EntityGraphQL requires a lambda expression to  
                var ctxExpression = Expression.Parameter(typeof(DbContext), "ctx");
                var paramsExpression = Expression.Parameter(paramsType, "param");

                // Param needs to be used as a variable in the body of the expression.
                var paramVariable = Expression.Variable(paramsType, "param");

                // The invoker is created in the bootstrap
                var invokerConstant = Expression.Constant(serviceMethodInvoker);

                // Get the methods used in the expressions
                var containerGetMethod = container.MethodNamed("GetService", typeof(Type))
                                         ?? throw new Foundry.ReflectionException(
                                             $"{nameof(GraphBootstrapService)} cannot find Get method on Container");
                var invokerInvokeMethod = serviceMethodInvoker.MethodNamed("Invoke")
                                          ?? throw new Foundry.ReflectionException(
                                              $"{nameof(GraphBootstrapService)} cannot find Invoker method on IMethodInvoker");
                var exploderMethod = typeof(ReflectionExtensions).GetMethod("ExplodeProperties")
                                     ?? throw new Foundry.ReflectionException(
                                         $"{nameof(GraphBootstrapService)} cannot find Explode Properties Method");

                /*
                 * The properties on the params instance need to be
                 * sent to the method in an object array. This will call
                 * ObjectExtensions.ExplodeProperties which will facilitate that.
                 */
                var objectExtensionsExplodePropertiesMethod =
                    Expression.Call(null,
                        exploderMethod,
                        paramVariable);

                /*
                 * Get the ManagedType that this method is declared on from the container. This will
                 * allow for Autowiring to happen.
                 */
                var containerGetMethodExpression =
                    Expression.Call(Expression.Constant(container),
                        containerGetMethod,
                        Expression.Constant(method.DeclaringType));

                /*
                 * Method expression to call the Invoker that will actually send run the method on the ManagedType.
                 */
                var invokerMethodCall = Expression.Call(invokerConstant,
                    invokerInvokeMethod,
                    containerGetMethodExpression, objectExtensionsExplodePropertiesMethod);

                // Cast as the ManagedType method's return value
                var castExpression = Expression.Convert(invokerMethodCall, method.ReturnType);

                // return the full expression with the context and params.
                return Expression.Lambda(castExpression, new[] {ctxExpression, paramsExpression});

            }
        }
    }
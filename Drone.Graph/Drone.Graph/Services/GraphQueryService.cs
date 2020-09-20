using System;
using EntityGraphQL;
using EntityGraphQL.Compiler;
using EntityGraphQL.LinqQuery;
using EntityGraphQL.Schema;
using Microsoft.EntityFrameworkCore;
using Rigger.Attributes;
using Rigger.Exceptions;

namespace Drone.Graph.Services
{
    [Singleton]
    public class GraphQueryService : IGraphQueryService
    {
        [Autowire] private DbContext dbContext;
        [Autowire] private ISchemaProvider schema;

        private GraphQLCompiler compiler;

        /// <summary>
        /// Create the graphql compiler when this service is created in the container.
        /// </summary>
        [OnCreate]
        private void OnCreate()
        {
            if (schema == null)
                throw new ContainerException("Cannot instantiate GraphQL service. Did you register a DbContext?");

            compiler = new GraphQLCompiler(schema,
                new DefaultMethodProvider());

        }

        /// <summary>
        /// Make a Query Result from a Query Request
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <returns>The result of the query. If there are errors, they will be added to the Error list on the result</returns>
        public QueryResult MakeResult(QueryRequest request)
        {
            try
            {
                var queryResult = compiler.Compile(request);


                return queryResult.ExecuteQuery(dbContext);
            }
            catch (Exception ex)
            {
                return new QueryResult
                {
                    Errors = {new GraphQLError(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}
                };
            }
        }
    }
}
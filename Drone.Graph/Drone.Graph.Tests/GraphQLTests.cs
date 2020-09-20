using System;
using EntityGraphQL;
using EntityGraphQL.Compiler;
using EntityGraphQL.LinqQuery;
using EntityGraphQL.Schema;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rigger;
using Rigger.API.Attributes;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.Implementations;
using Rigger.Injection;
using Rigger.ManagedTypes;
using Xunit;
using Xunit.Abstractions;

namespace Drone.Graph.Tests
{
    
    public class TestContainer : Rig 
    {
    }
    public static class LoggerOutput
        { 
            public static ITestOutputHelper Output { get; set; }
        }

        [Module]
        public class TestModule
        {
            public TestModule(IServices ctx)
            {
                ctx.Add<ILogger<TestContainer>>(new TestLogger(LoggerOutput.Output).Logger);
            }
        }

        [Scalar]
        [Managed]
        [SchemaInformation(Name = "Boo", Description = "Scalar Test Type")]
        public class ScalarType
        {
            private string Test { get; set; }

            public override string ToString()
            {
                return Test;
            }
        }

        public class TestEntity
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
           public string Text { get; set; }
        }

        public class TestContext : DbContext
        {
            public DbSet<TestEntity> Entities { get; set; }
            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {

            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<TestEntity>().HasKey(o => o.Id);
            }
        }

        [External]
        [Managed]
        public class QueryReturn
        {
            public string QueryReturnInfo { get; set; }
        }

        [Singleton]
        public class QueryService
        {
            [Query]
            [SchemaInformation(Name="testQuery", Description = "This is a test query")]
            string QueryString(string one, string two)
            {
                return one;
            }
            [Query]
            [SchemaInformation(Name = "testQuery2", Description = "This is another test query")]
            QueryReturn QueryString2(string one, string two)
            {
                return new QueryReturn { QueryReturnInfo = $"This is query return ({one},{two})"};
            }
        }


        public class MutateArgs
        {
            public string Value;
        }

        [Mutator]
        [Singleton]
        public class MutatorService
        {
            [Autowire]
            private IContainer container;

            [GraphQLMutation("This is a test mutator")]
            public QueryReturn MutateData(DbContext ctx, MutateArgs args)
            {
                return new QueryReturn
                {
                    QueryReturnInfo = args.Value
                };
            }
        }

        [Bootstrap] 
        public class ConfigureDb
        {
            [Autowire] private IContainer container;

            [OnCreate]
            public void ConfigureDbStartup()
            {
                var optionsBuilder = new DbContextOptionsBuilder<TestContext>();

                optionsBuilder.UseInMemoryDatabase("Test-Db");
                
                var ctx = new TestContext(optionsBuilder.Options);
                
                container.Register<DbContext>(ctx);
            }
        }

    public class GraphQLTests
    {

        public GraphQLTests(ITestOutputHelper output)
        {
            LoggerOutput.Output = output;
        }

        [Fact]
        public void TestGraphQLModule()
        {
            var container = new TestContainer();

            var schema = container.Get<ISchemaProvider>();
            var dbContext = container.Get<DbContext>();

            schema.Should().NotBeNull();

            var graphQlCompiler = new GraphQLCompiler(schema, new DefaultMethodProvider());
            var introspection = @"query IntrospectionQuery { __schema { queryType { name } mutationType { name } subscriptionType { name } types { ...FullType } directives { name description locations args { ...InputValue } } } } fragment FullType on __Type { kind name description fields(includeDeprecated: true) { name description args { ...InputValue } type { ...TypeRef } isDeprecated deprecationReason } inputFields { ...InputValue } interfaces { ...TypeRef } enumValues(includeDeprecated: true) { name description isDeprecated deprecationReason } possibleTypes { ...TypeRef } } fragment InputValue on __InputValue { name description type { ...TypeRef } defaultValue } fragment TypeRef on __Type { kind name ofType { kind name ofType { kind name ofType { kind name ofType { kind name ofType { kind name ofType { kind name ofType { kind name } } } } } } } } ";
            
            var introspectionCompiled = graphQlCompiler.Compile(new QueryRequest { OperationName = "IntrospectionQuery", Query = introspection } );

            var introspectionResult = introspectionCompiled.ExecuteQuery(dbContext);

            var queryResult = graphQlCompiler.Compile(new QueryRequest()
            {
                OperationName = "query",
                Query = @"{ testQuery2(one:""x"", two:""y"") { queryReturnInfo } }"
            });

            var result = queryResult.ExecuteQuery(dbContext);

            result.Data.ForEach(k =>
            {
                k.Key.Should().Be("testQuery2");
                k.Value.FieldValue("queryReturnInfo").Should().Be("This is query return (x,y)");
            });

            var queryResult2 = graphQlCompiler.Compile(new QueryRequest()
            {
                OperationName = "mutation",
                Query = @"mutation mutateData($value: String) { mutateData(value: $value) { queryReturnInfo } }",
                Variables = new QueryVariables() { { "value", "123"} }
            });
            
            result = queryResult2.ExecuteQuery(dbContext);

            result.Data.ForEach(k =>
            {
                k.Key.Should().Be("mutateData");
                k.Value.FieldValue("queryReturnInfo").Should().Be("123");
            });
        }
    }
}
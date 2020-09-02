using System.Collections.Generic;
using FluentAssertions;
using TheCommons.Core.Configuration;
using TheCommons.Core.Configuration.Sources;
using TheCommons.Forge.ManagedTypes.Features;
using TheCommons.Forge.ManagedTypes.Resolvers;
using Xunit;

namespace TheCommons.Forge.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void TestValueInjectionFromConfiguration()
        {

        }

        class Blue { }
        class Red { }

        /// <summary>
        /// Test the expression parser and expression resolver. Tihs allows us to use a configuration
        /// system to choose which type to return.
        /// </summary>
        [Fact]
        public void TestExpressionParser()
        {
            var configuration = new Dictionary<string, object> { {"Color","Blue"} };

            var autowirer = new MapAutowirer();

            autowirer.Map.Add(typeof(IConfigurationService), new DefaultConfigurationService()
                .AddSource(new MapConfigurationSource(){ BackingMap = configuration}));

            var resolver = autowirer.Inject(new ExpressionTypeResolver());

            resolver.AddType(@"Color == Blue", typeof(Blue));
            resolver.AddType(@"Color == Red", typeof(Red));

            resolver.ResolveType().Should().Be<Blue>();
        }
    }
}
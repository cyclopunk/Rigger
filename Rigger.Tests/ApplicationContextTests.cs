using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheCommons.Core.Configuration;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Features;
using Xunit;
using Xunit.Abstractions;

namespace TheCommons.Forge.Tests
{
    public class ApplicationContextTests
    {
        private ITestOutputHelper _output;
        private Mock<ILogger> _logger;

        public ApplicationContextTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new Mock<ILogger>();
        }

        public class TestAutowireClass
        {
            public IContainer Container { get;  set; }
            public ILogger Logger { get; set; }
            public IConfigurationService ConfigurationService { get; set; }
            public ITypeRegistry TypeRegistry { get;  set; }
        }

        [Fact]
        public void TestApplicationContextInitDefaults()
        {
           var container = new ManagedTypes.Crucible();
           container.Build(typeof(DefaultModule).Assembly);

           container.Context.Logger = _logger.Object;

           container.Context.Validate();
            
        }

        [Fact]
        public void TestAutowireOfContext()
        {
            ManagedTypes.Crucible container = new ManagedTypes.Crucible();
            container.Build(typeof(DefaultModule).Assembly);
            var ctx = container.Context;

            ctx.Logger = _logger.Object;
            ctx.Logger.LogInformation("This is a test");

            TestAutowireClass awc = ctx.Inject(new TestAutowireClass());

            awc.Container.Should().NotBe(null);
            awc.ConfigurationService.Should().NotBe(null);
            awc.Logger.Should().NotBe(null);
            awc.TypeRegistry.Should().NotBe(null);
        }
    }
}
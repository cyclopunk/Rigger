using Drone;
using Drone.Tests;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rigger.Attributes;
using Rigger.Implementations;
using Rigger.Injection;
using Rigger.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Drone.Tests
{
   [Singleton]
   public class TestService { }

   [Bootstrap]
   public class BootstrapService
   {
       [Autowire] public TestService testService;
       [Autowire] public ILogger<BootstrapService> logger;
       public bool Created { get; set; }
       [OnCreate]
       public void OnCreate()
       {
           logger.LogInformation("Bootstrap!");
           Created = true;
       }
   }

   [Module(Priority = 1)]
   public class TestModule
   {
       public TestModule(IServices services)
       {
           services.Replace<ILoggerFactory, ILoggerFactory>(new TestLogger(TestLog.output).LoggerFactory);
       }
   }
}
namespace Rigger.Tests
{
    public static class TestLog
    {
        public static ITestOutputHelper output;
    }
    public class RiggedApp : Rig
    {
        // this app will scan all of the components from the namespace "Drone.Tests"
        public RiggedApp() : base("Drone.Tests")
        {

        }
    }
    public class RiggerTests
    {
        public RiggerTests(ITestOutputHelper output)
        {
            TestLog.output = output;
        }
        [Fact]
        public void PutItAllTogether()
        {
            RiggedApp rig = new RiggedApp();
            
            rig.Get<ILogger<RiggedApp>>().LogInformation("After Start!");
            rig.Get<TestService>().Should().NotBeNull();
            
            var bService = rig.Get<BootstrapService>();
            
            rig.Get<ILogger<RiggedApp>>().LogInformation("AfterBootstrap!");
            
            bService.Should().NotBeNull();
            bService.testService.Should().NotBeNull();
        }
    }
}
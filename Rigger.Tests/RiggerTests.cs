using Drone;
using Drone.Tests;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rigger.Attributes;
using Rigger.Implementations;
using Rigger.Injection;
using Rigger.Tests;
using System;
using Rigger.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Drone.Tests
{
    public interface IConditionalService
    {
        public string GetColor();
    }
    [Managed]
    [Condition("ServiceColor == Blue")]
    public class BlueService : IConditionalService
    {
        public string GetColor()
        {
            return "Blue";
        }
    }
    [Managed]
    [Condition("ServiceColor == Green")]
    public class GreenService : IConditionalService
    {
        public string GetColor()
        {
            return "Green";
        }
    }


    [Managed]
    public class ValueService {

        [Value]
        public string ConfigValue { get; set; }

        [Value(Key = "AnotherValue")]
        public int AnotherValue2;
    }

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

           //services.Replace<ILoggerFactory, ILoggerFactory>(new TestLogger(TestLog.output).LoggerFactory);
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
        public void TestConditionalServices()
        {
            Environment.SetEnvironmentVariable("ServiceColor", "Green");

            RiggedApp rig = new RiggedApp();

            rig.Get<IConfigurationService>().Get("ServiceColor").Should().Be("Green");

            //rig.Register<IConditionalService, BlueService>();
            //rig.Register<IConditionalService, GreenService>();

            var conditionalService = rig.Get<IConditionalService>();

            conditionalService.GetColor().Should().Be("Green");
        }
        [Fact]
        public void TestValueInjection()
        {
            Environment.SetEnvironmentVariable("ConfigValue", "Test");
            Environment.SetEnvironmentVariable("AnotherValue", "1");
            RiggedApp rig = new RiggedApp();
            rig.Register<ValueService, ValueService>();

            var service = rig.Get<ValueService>();

            service.ConfigValue.Should().Be("Test");
            service.AnotherValue2.Should().Be(1);
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
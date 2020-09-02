using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheCommons.Core.Configuration;
using TheCommons.Core.Configuration.Sources;
using TheCommons.Core.Reflection;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Logging.Implementations;
using TheCommons.Traits.Attributes;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TheCommons.Forge.Tests
{

    public class ApplicationTests 
    {
        public ApplicationTests(ITestOutputHelper output)
        {
            LoggerOutput.Output = output;
        }

        public static class LoggerOutput
        {
            public static ITestOutputHelper Output { get; set; }
        }

        [Module]
        public class TestModule
        {
            public TestModule(ApplicationContext ctx)
            {
                if (LoggerOutput.Output != null)
                {
                    ctx.Logger = new TestLogger(LoggerOutput.Output).Logger;
                }
            }
        }
        [Configuration]
        public class DefaultApplicationConfiguration
        {
            public bool DidConfigure { get; set; }
            [Autowire]
            public DefaultApplicationConfiguration(IContainer container, IConfigurationService configuration)
            {
                //container.Register<IValueResolver, ConfigurationValueResolver>();
                configuration.AddSource(new MapConfigurationSource
                {
                    BackingMap = new Dictionary<string, object>
                    {
                        {"TestValue", "This Is A Test Value"},
                        { "BoolValue", "true" }
                    }
                });
                DidConfigure = true;
            }
        }
        public static string TEST_STRING = "This is a test";
        public static string TEST_EVENT_MESSAGE = "This is a test message string";

        public class EventTestClass
        {
            public string Message { get; set; }
        }

        [Bootstrap]
        public class TestBootstrap
        {
            [Autowire] private ILogger _logger;
            public bool OnCreateCalled { get; set; }
            public bool OnStartupCalled { get; set; }
            public bool OnDestroyCalled { get; set; }
            public static bool OnBuiltCalled { get; set; }

            public string LastMessage { get; set; }

            [OnBuilt]
            public static void OnBuilt()
            {
                OnBuiltCalled = true;
            }

            [OnCreate]
            void OnCreateCaller()
            {
                _logger?.LogInformation("OnCreateCaller called during OnCreate lifecycle");
                OnCreateCalled = true;
            }
            [OnStartup]
            void OnStartupCaller()
            {
                OnStartupCalled = true;
            }

            [OnDestroy]
            void OnDestoryCaller()
            {
                OnDestroyCalled = true;
            }
            [OnEvent(Event=typeof(EventTestClass))]
            void OnEventSent(IAnotherService service, EventTestClass evt)
            {
                service.Should().NotBeNull();
                LastMessage = evt.Message;
            }
        }

        public interface ITestService
        {
            object DoAnotherSomething();
        }
        public class OpenService <TEntity> : IOpenService<TEntity>
        {
            public Type MyType()
            {
                return this.GetType().GetInterfaces()[0].GetGenericArguments()[0];
            }
        }
        public interface IOpenService<TEntity>
        {
            public Type MyType();
        }
     

        public interface IAnotherService
        {
            void DoSomething();
            object ReturnAValue();
        }

        [Managed]
        public class AnotherService : IAnotherService
        {
            private string _value = "";
            [Value(Key="TestValue")] public string TestFieldValue;
            [Value] public bool BoolValue;
            [Autowire] private ITestService service;

            public void DoSomething()
            {
                _value = TEST_STRING;
            }

            public object ReturnAValue()
            {
                return _value;
            }
            [OnDestroy]
            public void DestroyMe()
            {

            }
        }

        [Singleton]
        public class TestService : ITestService
        {
            [Autowire] private IAnotherService _anotherService;
            
            [Value(Key="TestValue")] public string TestPropertyValueAttribute { get; set; }
            [Value(Key="TestValue")] public string TestFieldValue;

            [OnCreate]
            private void OnCreate()
            {
                _anotherService.DoSomething();
            }
            public object DoAnotherSomething()
            {
                return _anotherService.ReturnAValue();
            }
        }

        class CrucibleContainer : Crucible
        {
            // only load the test as an Ingot
            public CrucibleContainer() : base(typeof(ApplicationTests))
            {

            }
        }

        [Fact]
        void TestApplicationContainer()
        {

            LoggerOutput.Output.WriteLine("Starting test of Application with a singleton, managed service, configuration and a handler.");

            CrucibleContainer container =
                new CrucibleContainer();

            ITestService its = container.Get<ITestService>();

            its.Should().NotBeNull();

            its.Should().BeAssignableTo<TestService>();

            its.DoAnotherSomething().Should().Be(TEST_STRING);

            if (its is TestService ts)
            {
                ts.TestFieldValue.Should().Be("This Is A Test Value");
                ts.TestPropertyValueAttribute.Should().Be("This Is A Test Value");
            }

            TestBootstrap testBootstrap = container.Get<TestBootstrap>();

            testBootstrap.OnStartupCalled.Should().BeTrue();
            testBootstrap.OnCreateCalled.Should().BeTrue();
            TestBootstrap.OnBuiltCalled.Should().BeTrue();


            container.Context.EventRegistry.Fire(new EventTestClass { Message = TEST_EVENT_MESSAGE});

            testBootstrap.LastMessage.Should().Be(TEST_EVENT_MESSAGE);

            DefaultApplicationConfiguration config = container.Get<DefaultApplicationConfiguration>();

            config.DidConfigure.Should().BeTrue();

            AnotherService anotherService = (AnotherService) container.Get<IAnotherService>();
            anotherService.BoolValue.Should().BeTrue();
        }

        [Fact]
        public void TestOpenRegistration()
        {
            CrucibleContainer container =
                new CrucibleContainer();

            container.Register(RegistrationType.Transient, typeof(IOpenService<>), typeof(OpenService<>));
            
            var g = container.Get<IOpenService<AnotherService>>();
            
            g.MyType().Should().BeAssignableTo<AnotherService>();
        }

        /*public class BenchmarkLogger : ILogger
        {
            public void Write(LogKind logKind, string text)
            {
                LoggerOutput.Output.WriteLine(text);
            }

            public void WriteLine()
            {
                LoggerOutput.Output.WriteLine("");
            }

            public void WriteLine(LogKind logKind, string text)
            {
                LoggerOutput.Output.WriteLine(text);
            }

            public void Flush()
            {
                // NOP
            }
        }*/

        [Fact]
        public void TestAppScan()
        {
            var c = new Crucible("TheCommons.Forge");

        }
        public static class ApplicationHolder
        {
            public static IContainer Application = new ManagedTypes.Crucible(typeof(ApplicationTests));
            //public static Container container = new Container();
            
            
        }

        public class BenchmarkConfig : ManualConfig
        {
            public BenchmarkConfig()
            {
                //Add(new BenchmarkLogger());

                Add(Job.ShortRun
                    .WithLaunchCount(1)
                    .With(InProcessEmitToolchain.Instance)
                    .WithId("InProcess"));
            }
        }
        void RunBenchmark()
        {
            var TEST_RUNS = 200000;


            //ApplicationHolder.container.Register<IAnotherService, AnotherService>();

            var start = ticks();
            for (int i = 0; i < TEST_RUNS; i++)
            {
                ApplicationHolder.Application.Get<ITestService>();
            }

            LoggerOutput.Output.WriteLine($"Ticks to create {TEST_RUNS} managed instances: {ticks() - start}");

            start = ticks();
            for (int i = 0; i < TEST_RUNS; i++)
            {
                var x = ApplicationHolder.Application.Get<IAnotherService>();
            }
            LoggerOutput.Output.WriteLine($"Ticks to create {TEST_RUNS} transient instances {ticks() - start}");

            ExpressionActivator<AnotherService> activator = new ExpressionActivator<AnotherService>();

            start = ticks();
            for (int i = 0; i < TEST_RUNS; i++)
            {
                activator.Activate();
            }
            LoggerOutput.Output.WriteLine($"Ticks to create {TEST_RUNS} expression activator instances: {ticks() - start}");

            start = ticks();
            for (int i = 0; i < TEST_RUNS; i++)
            {
                new AnotherService();
            }
            LoggerOutput.Output.WriteLine($"Ticks to create {TEST_RUNS} new instances: {ticks() - start}");

            start = ticks();
            for (int i = 0; i < TEST_RUNS; i++)
            {
               // ApplicationHolder.container.GetInstance<IAnotherService>();
            }
            LoggerOutput.Output.WriteLine($"Ticks to create {TEST_RUNS} simple injector instances {ticks() - start}");

        }

        public int ticks()
        {
          return Environment.TickCount & Int32.MaxValue;
        }

        [HtmlExporter]
        [NamespaceColumn]
        [AllStatisticsColumn]
        public class BenchmarkInstanceCreate
        {
            private const int N = 100;
            public BenchmarkInstanceCreate()
            {
            }

            //[Benchmark]
            public ITestService GetTestServiceManaged()
            {
               
                return ApplicationHolder.Application.Get<ITestService>();
            }
            [Benchmark]
            public IAnotherService GetAnotherServiceManaged()
            {
                return ApplicationHolder.Application.Get<IAnotherService>();
            }
            [Benchmark]
            public object GetTestServiceFastActivator()
            {
                return null; //ApplicationHolder.AnotherServiceActivator();
            }

            //[Benchmark]
            public AnotherService GetTestServiceNotManaged() => new AnotherService();
            
            /*[Benchmark]
            public IAnotherService GetTransientSimpleInjectorTestService() =>
                ApplicationHolder.container.GetInstance<IAnotherService>();*/
        }
    }

    
}
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Loggers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Features;
using TheCommons.Forge.ManagedTypes.Implementations;
using TheCommons.Forge.ManagedTypes.Lightweight;
using TheCommons.Logging.Implementations;
using TheCommons.Traits.Attributes;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TheCommons.Forge.Tests {
    public interface ISingletonService
{
}
   public class SingletonService : ISingletonService
    {
    }
    public class TransientService
    {
        public SingletonService Option { get; set; }

        public TransientService(SingletonService o)
        {
            Option = o;
        }
    }

    public class AutoWireService
    {
        [Autowire] public ISingletonService service;
    }

    public class SomeOption
    {
    }
    public class AnotherOption
    {

    }
    public class OneMoreOption
    {

    }
    public class OptionOne : IOption<SomeOption>
    {
        public IOption<AnotherOption> Option { get; set; }

        public OptionOne(IOption<AnotherOption> o)
        {
            Option = o;
        }
        public bool IsTrue { get; set; } = true;
    }
    public class OptionTwo : IOption<AnotherOption>
    {
        public bool IsTrue { get; set; } = false;

        public OptionTwo()
        {

        }

        public OptionTwo(IOption<OneMoreOption> op)
        {
            this.IsTrue = op.IsTrue;
        }
    }
    public interface IOption<T>
    {
        public bool IsTrue { get; set; }
    } 
    public class LightweightServiceTests
    {
        private readonly ITestOutputHelper output;
        public LightweightServiceTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        /// <summary>
        /// Extension method tests that validate and offer comparisons between service descriptions.
        /// </summary>
        [Fact]
        public void TestExtensionMethods()
        {
            var sd0 = new ServiceDescription { ServiceType = typeof(IOption<>), ImplementationType = typeof(OptionOne) };
            var sd1 = new ServiceDescription { ServiceType = typeof(IOption<SomeOption>), ImplementationType = typeof(OptionOne) };

            var sd2 = new ServiceDescription
                {ServiceType = typeof(IOption<AnotherOption>), ImplementationType = typeof(OptionTwo)};

            sd1.IsValid().Should().BeTrue();
            sd2.IsValid().Should().BeTrue();
            
            // concrete type tests
            new ServiceDescription { ServiceType = typeof(OptionOne), ImplementationType = typeof(OptionOne) }.IsValid().Should().BeTrue();
            new ServiceDescription { ServiceType = typeof(OptionTwo), ImplementationType = typeof(OptionOne) }.IsValid().Should().BeFalse();
            new ServiceDescription { ServiceType = typeof(OptionOne), ImplementationType = typeof(OptionTwo) }.IsValid().Should().BeFalse();
            new ServiceDescription {ServiceType = typeof(OptionOne), ImplementationType = typeof(IOption<>)}.IsValid()
                .Should().BeFalse();
            new ServiceDescription { ServiceType = typeof(OptionOne), ImplementationType = typeof(IOption<SomeOption>) }.IsValid().Should().BeFalse();

            // open type test
            new ServiceDescription { ServiceType = typeof(IOption<>), ImplementationType = typeof(OptionOne) }.IsValid().Should().BeTrue();

            // specific type tests
            new ServiceDescription { ServiceType = typeof(IOption<AnotherOption>), ImplementationType = typeof(OptionOne) }.IsValid().Should().BeFalse();
            new ServiceDescription { ServiceType = typeof(IOption<AnotherOption>), ImplementationType = typeof(IOption<AnotherOption>) }.IsValid().Should().BeFalse();

            sd0.MoreSpecificService(sd1).Should().BeEquivalentTo(sd1);
        }

        private Services InitServices()
        {
            return new Services()
                .Add<IAutowirer>(new ContainerAutowirer())
                .Add<ILogger>(new TestLogger(output).Logger)
                .Add<IConstructorInvoker>(new ManagedConstructorInvoker())
                .Add<IInstanceFactory>(new AutowireInstanceFactory())
                .Add<IOption<SomeOption>, OptionOne>()
                .Add<IOption<AnotherOption>, OptionTwo>()
                .Add<ISingletonService, SingletonService>(ManagedTypes.Lightweight.ServiceLifecycle.Singleton)
                .Add<TransientService, TransientService>();
        }
        public class OptionThree : IOption<SomeOption>
        {
            public bool IsTrue
            {
                get => false;
                set { }
            }
        }

        [Fact]
        public void TestEnumerations()
        {
            
            var services = InitServices().Add<IOption<SomeOption>, OptionThree>();

            var list = services.GetService(typeof(IEnumerable<IOption<SomeOption>>));
            
            list.Should().BeAssignableTo<List<IOption<SomeOption>>>();

            if (list is List<IOption<SomeOption>> i)
            {
                i.Count.Should().Be(2);
            }
        }

        class OpenGeneric<TInstance> : IOpenGeneric<TInstance>
        {
            public TInstance Property { get; set; }
        }

        interface IOpenGeneric<TInstance>
        {
            public TInstance Property { get; set; }

        } 
        
        [Fact]
        public void TestAutowire()
        {
            
            var services = InitServices().Add<AutoWireService,AutoWireService>();

            var aw = services.GetService<AutoWireService>();

            aw.Should().BeAssignableTo<AutoWireService>();

            aw.service.Should().BeAssignableTo<ISingletonService>();
        }

        [Fact]
        public void TestOpenTypes()
        {
            
            var services = InitServices().Add(typeof(IOpenGeneric<>),typeof(OpenGeneric<>));

            var logger = services.GetService<IOpenGeneric<ILogger>>();
            var opt = services.GetService<IOpenGeneric<IOption<OptionOne>>>();

            logger.Should().BeAssignableTo<OpenGeneric<ILogger>>();

            opt.Should().BeAssignableTo<OpenGeneric<IOption<OptionOne>>>();
        }
        [Fact]
        public void TestInstanceConstruction()
        {
            var services = InitServices();

            services.Validate().Should().BeEmpty();

            var x = services.GetService<IOption<SomeOption>>();
            var y = services.GetService<IOption<AnotherOption>>();

            if (x is OptionOne oo)
            {
                oo.Option.Should().NotBeNull();
            }

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100000; i++)
            {
                services.GetService<IOption<AnotherOption>>();
            }

            sw.Stop();
            services.GetService<ILogger>().LogInformation("Time to make 100k non-autowired objects: " + sw.ElapsedMilliseconds + "ms");
            sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                services.GetService<ISingletonService>();
            }

            sw.Stop();
            
            services.GetService<ILogger>().LogInformation("Time to make 100k autowire w/ singleton objects: " + sw.ElapsedMilliseconds + "ms");
            sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                var op2 = new OptionOne(new OptionTwo());
            }

            sw.Stop();

            services.GetService<ILogger>().LogInformation("Time to make 100k natural objects: " + sw.ElapsedMilliseconds + "ms");
            

            x.Should().NotBeNull();
            y.Should().NotBeNull();
        }
    }
}
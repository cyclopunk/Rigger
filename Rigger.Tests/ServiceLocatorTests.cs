using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Features;
using TheCommons.Forge.ManagedTypes.ServiceLocator;
using Xunit;

namespace TheCommons.Forge.Tests
{
    public class ConcreteLogger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
    public class TestGenericClass<T>
    {

    }

    public interface TestGenericInterface<T>
    {

    }

    public class TestGenericService <T> : TestGenericInterface<T>
    {

    }
    public class TestNonGenericService : TestGenericInterface<ServiceLocatorTests>
    {

    }
    public class ServiceLocatorTests
    {
        [Fact]
        public void TestServiceType()
        {
            IContainer container = new Mock<IContainer>().Object;
            ManagedTypeRegistry registry = new ManagedTypeRegistry();
            

            var openGeneric = typeof(ILogger<>);
            var openGenericConcrete = typeof(ConcreteLogger<>);
            var concreteClassWithGeneric = typeof(TestGenericService<TestNonGenericService>);
            var concreteClassWithoutGeneric = typeof(TestNonGenericService);
            
            var stLogger = new ServiceType(container, openGeneric, openGenericConcrete);

            stLogger.IsConcrete.Should().BeFalse();
            stLogger.CanInstantiate.Should().BeFalse();
            stLogger.IsOpenGeneric.Should().BeTrue();
            stLogger.IsOfType(openGeneric).Should().BeTrue();
            stLogger.IsParentOf(openGenericConcrete).Should().BeTrue();

        }
        
        [Fact]
        public void TestManagedTypeRegistry()
        {
            IContainer container = new Mock<IContainer>().Object;
            
            ManagedTypeRegistry registry = new MapAutowirer{ 
                Map = new Dictionary<Type,object>
                {
                    {typeof(IContainer), container}

                }
            }.Inject(new ManagedTypeRegistry());


            var openGeneric = typeof(ILogger<>);
            var openGenericConcrete = typeof(ConcreteLogger<>);
            var closedGenericConcrete = typeof(ConcreteLogger<TestNonGenericService>);
            var openGenericService = typeof(TestGenericService<>);
            var closedGenericService = typeof(TestGenericService<TestNonGenericService>);
            var concreteClassWithoutGeneric = typeof(TestNonGenericService);

            registry.Register(RegistrationType.Singleton, openGeneric, openGenericConcrete);
            registry.Register(RegistrationType.Transient, openGenericService);
            registry.Register(RegistrationType.Thread, concreteClassWithoutGeneric);

            var stLogger = registry.Get(openGeneric);
            var stService = registry.Get(openGenericService);
            var stConcrete = registry.Get(concreteClassWithoutGeneric);
            
            stLogger.Should().NotBeNull();
            stLogger.LookupType.Should().Be(openGeneric);
            stLogger.ImplementationType
                .UnderlyingType.Should().Be(openGenericConcrete);

            stService.Should().NotBeNull();
            stService.LookupType.Should().Be(openGenericService);
            stService.ImplementationType
                .UnderlyingType.Should().Be(openGenericService);

            stConcrete.Should().NotBeNull();

            var closedType = stLogger.ImplementationType.CloseType(typeof(TestNonGenericService));
            var closedService = stService.ImplementationType.CloseType(typeof(TestNonGenericService));

            closedType.UnderlyingType.Should().BeAssignableTo<ConcreteLogger<TestNonGenericService>>();
            closedService.UnderlyingType.Should().BeAssignableTo<TestGenericService<TestNonGenericService>>();

            new Func<object>(() => stLogger.ImplementationType.Instantiate()).Should()
                .Throw<Exception>();
            new Func<object>(() => stService.ImplementationType.Instantiate()).Should()
                .Throw<Exception>();

            new Func<object>(() => closedType.Instantiate()).Should()
                .NotThrow();
            new Func<object>(() => closedService.Instantiate()).Should()
                .NotThrow();

            closedType.Instantiate().Should().Be(closedType.Instantiate());
            closedService.Instantiate().Should().NotBe(closedService.Instantiate());

            object objThread1 = null;
            object objThread2 = null;
            
            Action action = () =>
            {
                objThread1 = stConcrete.Instantiate();
                objThread1.Should().Be(stConcrete.Instantiate());
            };
            Action action2 = () =>
            {
                objThread2 = stConcrete.Instantiate();
                objThread2.Should().Be(stConcrete.Instantiate());
            };

            Parallel.Invoke(action2, action);

            objThread1.Should().NotBe(objThread2);
        }
    }
}
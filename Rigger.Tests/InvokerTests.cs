using FluentAssertions;
using TheCommons.Core.Reflection;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Features;
using TheCommons.Forge.ManagedTypes.Implementations;
using Xunit;

namespace TheCommons.Forge.Tests
{
    public class InvokerTests
    {
        interface IInvoked
        {

        }

        class InvokedImpl : IInvoked
        {
            public static bool StaticMethodInvoked { get; set; }
            public static void StaticMethodToInvoke(InvokeService s)
            {
                StaticMethodInvoked = true;
                s.Should().NotBeNull();
            }

            InvokeService MethodToInvoke(InvokeService s, string providedString)
            {
                s.ProvidedString = providedString;
                return s;
            }
        }

        class InvokeService
        {
            public string ProvidedString { get; set; }

        }

        class StaticFieldTestObject
        {
            private static string Field1;
            private static string Property1 { get; set; }
        }

        private ManagedTypes.Crucible container;

        IContainer GetContainer()
        {
            return container ??= new ManagedTypes.Crucible();
        }

        [Fact]
        void TestStaticPropertyAndFieldInvoke()
        {
            var fieldAccessor = new ExpressionFieldAccessor(typeof(StaticFieldTestObject), "Field1");
            var propAccessor = new ExpressionPropertyAccessor(typeof(StaticFieldTestObject), "Property1");

            fieldAccessor.SetValue(new StaticFieldTestObject(), "1");
            propAccessor.SetValue(new StaticFieldTestObject(), "2");

            fieldAccessor.GetValue(new StaticFieldTestObject()).Should().Be("1");
            propAccessor.GetValue(new StaticFieldTestObject()).Should().Be("2");
        }
        /// <summary>
        /// A managed invoker allows for a container to inject Managed Type instances into a method
        /// from the managed container.
        /// </summary>
        [Fact]
        void TestManagedInvoker()
        {;

            GetContainer();

            container.Build(typeof(DefaultModule).Assembly);

            var TEST_STRING = "Provided String";

            container.Register<InvokeService>(new InvokeService());
            container.Register<IInvoked>(new InvokedImpl());

            var invoker = container.Context.Inject(new ManagedMethodInvoker(typeof(InvokedImpl), "MethodToInvoke"));
            var staticInvoker = container.Context.Inject(new ManagedMethodInvoker(typeof(InvokedImpl), "StaticMethodToInvoke"));
            var result = invoker.Invoke(container.Get<IInvoked>(), TEST_STRING);

            result.Should().NotBeNull();
            result.Should().BeAssignableTo<InvokeService>();

            if (result is InvokeService ii)
            {
                ii.ProvidedString.Should().Be(TEST_STRING);
            }

            staticInvoker.Invoke(null);

            InvokedImpl.StaticMethodInvoked.Should().BeTrue();
        }

    }
}
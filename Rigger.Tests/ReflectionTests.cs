using FluentAssertions;
using TheCommons.Core.Reflection;
using Xunit;

namespace TheCommons.Forge.Tests
{
    class TestObject
    {
        public TestObject o;
        public TestObject prop { get; set; }
        public bool Method1()
        {
            return true;
        }

        public void Method2(TestObject o)
        {
            this.o = o;
        }

    }
    public class ReflectionTests
    {
        [Fact]
        void TestZeroMethodAccessor()
        {
            var zp = new ZeroParameterMethodAccessor(typeof(TestObject), "Method1");

            zp.Invoke(new TestObject()).Should().Be(true);
        }
        [Fact]
        void TestArgumentMethodAccessor()
        {
            var zp = new SingleParameterMethodAccessor(typeof(TestObject), "Method2");
            var o = new TestObject();

            zp.Invoke(o, o);
            o.o.Should().Be(o);
        }

        [Fact]
        void TestPropertyAccessor()
        {
            var zp = new ExpressionPropertyAccessor(typeof(TestObject), "prop");
            var o = new TestObject();

            zp.SetValue(o,o);
            zp.GetValue(o).Should().Be(o);
        }
        [Fact]
        void TestFieldAccessor()
        {
            var zp = new ExpressionFieldAccessor(typeof(TestObject), "o");
            var o = new TestObject();

            zp.SetValue(o, o);
            zp.GetValue(o).Should().Be(o);
        }
    }
}
using System.Reflection;
using FluentAssertions;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.ComponentHandlers;
using TheCommons.Traits.Attributes;
using Xunit;

namespace TheCommons.Forge.Tests
{
    public class AssemblyScanningTests
    {

        public class AssemblyTestIngot
        {
            [Singleton]
            class TestAssemblySingleton
            {

            }
        }

        [ContainerOptions(Empty = true)]
        class EmptyContainer : ManagedTypes.Crucible
        {

        }

        [Fact]
        public void TestAssemblyScanning ()
        {
            ManagedTypes.Crucible empty = new EmptyContainer();

            empty.IsBuilt.Should().BeFalse();
            empty.ScannedAssemblies.Should().BeNullOrEmpty();

            ManagedTypes.Crucible container = new ManagedTypes.Crucible();

            container.ScannedAssemblies.Should().OnlyHaveUniqueItems();
            container.ScannedAssemblies.Should().Contain(Assembly.GetExecutingAssembly());
            container.ScannedAssemblies.Should().Contain(typeof(ILifecycle).Assembly);

            container = new ManagedTypes.Crucible(typeof(AssemblyTestIngot));

            container.ScannedAssemblies.Should().Contain(Assembly.GetExecutingAssembly());
            container.ScannedAssemblies.Should().Contain(typeof(ILifecycle).Assembly);
        }
    }
}
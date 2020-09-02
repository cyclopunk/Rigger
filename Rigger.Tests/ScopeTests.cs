using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TheCommons.Forge.Core;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Traits.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace TheCommons.Forge.Tests
{
    /// <summary>
    /// Test the Scoping of objects in Crucible
    /// </summary>
    public class ScopeTests
    {
        private ITestOutputHelper output;

        public ScopeTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Managed]
        class TransientRegistration
        {

        }
        [Singleton]
        class SingletonRegistration
        {

        }

        [Managed(Scoped = true)]
        class ScopedRegistration
        {
            public string ID = Guid.NewGuid().ToString();
            public override string ToString()
            {
                return ID;
            }
        }

        [Fact]
        public void TestTransientScope()
        {
            Crucible container = new Crucible();

            container.LoadIngot<ScopeTests>();
            
            var instance = container.Get<TransientRegistration>();

            (instance != container.Get<TransientRegistration>()).Should().BeTrue();
        }
        [Fact]
        public void TestSingletonScope()
        {
            ManagedTypes.Crucible container = new ManagedTypes.Crucible(typeof(ScopeTests));

            var instance = container.Get<SingletonRegistration>();

            (instance == container.Get<SingletonRegistration>()).Should().BeTrue();
        }

        public class NestedClass // required for ingot
        {

        }
        [Fact]
        public void TestScopedScope()
        {
            IServiceProvider container = new ForgeContainerBuilder().Build("TheCommons.Forge.Tests");

            //container.LoadIngot<ScopeTests>();

            //container.Register(RegistrationType.Scoped, typeof(ScopedRegistration));

            ScopedRegistration instance1;
            ScopedRegistration instance2;

            using (var scope = container.GetService<IServiceScopeFactory>().CreateScope())
            {
                instance1 = (ScopedRegistration) scope.ServiceProvider.GetService(typeof(ScopedRegistration));
                instance2 = (ScopedRegistration) scope.ServiceProvider.GetService(typeof(ScopedRegistration));

                output.WriteLine($"{instance1} {instance2}");

                (instance1 == instance2).Should().BeTrue();
            }

            using (var scope = container.GetService<IServiceScopeFactory>().CreateScope())
            {
                var instance3 = (ScopedRegistration) scope.ServiceProvider.GetService(typeof(ScopedRegistration));
                
                output.WriteLine($"{instance1} {instance2} {instance3}");

                (instance1 != instance3).Should().BeTrue();
            }
        }

        [Fact]
        public async Task TestThreadScope()
        {
            ManagedTypes.Crucible container = new ManagedTypes.Crucible(typeof(ScopeTests));

            container.Register(RegistrationType.Thread, typeof(ScopedRegistration));

            ScopedRegistration instance1 = null;
            ScopedRegistration instance2 = null;
            ScopedRegistration instance3 = null;

            var task1 = Task.Run(() =>
            {
                instance1 = container.Get<ScopedRegistration>();
                instance2 = container.Get<ScopedRegistration>();
            });
            var task2 = Task.Run(() =>
            {
                instance3 = container.Get<ScopedRegistration>();
            });

            await Task.WhenAll(task1, task2);

            (instance1 == instance2 && instance1 != instance3).Should().BeTrue();
        }

    }

}
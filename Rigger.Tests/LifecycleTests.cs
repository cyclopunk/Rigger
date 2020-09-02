using FluentAssertions;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Traits.Attributes;
using Xunit;

namespace TheCommons.Forge.Tests
{
    /// <summary>
    /// Tests for all supported lifecycles.
    /// </summary>
    public class LifecycleTests
    {
        [Bootstrap]
        public class BootstrapService
        {
            public bool OnStartupCalled;
            public static bool OnBuiltCalled;
            public bool OnDestroyCalled;

            [OnStartup]
            public void OnStartup()
            {
                OnStartupCalled = true;
            }

            [OnBuilt]
            public static void OnBuilt()
            {
                OnBuiltCalled = true;
            }

            [OnDestroy]
            public void OnDestroy()
            {
                OnDestroyCalled = true;
            }
        }

        [Fact]
        public void TestAllLifecycles()
        {
            ManagedTypes.Crucible container = new ManagedTypes.Crucible(typeof(LifecycleTests));
            BootstrapService test = container.Get<BootstrapService>();
            using (container)
            {


                BootstrapService.OnBuiltCalled.Should()
                    .BeTrue();

                test.OnStartupCalled.Should()
                    .BeTrue();

                test.OnDestroyCalled.Should()
                    .BeFalse();

            }

            test.OnDestroyCalled
                .Should()
                .BeTrue();

        }
    }
}
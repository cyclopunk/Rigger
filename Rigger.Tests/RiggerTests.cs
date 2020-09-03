using Drone;
using FluentAssertions;
using Rigger.Attributes;
using Xunit;

namespace Drone
{
   [Singleton]
   public class TestService { }
}
namespace Rigger.Tests
{
    
    public class RiggedApp : Rig
    {

    }
    public class RiggerTests
    {
        [Fact]
        public void PutItAllTogether()
        {
            RiggedApp rig = new RiggedApp();

            rig.Get<TestService>().Should().NotBeNull();
        }
    }
}
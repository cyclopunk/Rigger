using Xunit;

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

        }
    }
}
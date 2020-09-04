using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rigger.Attributes;
using Rigger.Implementations;
using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.Features;
using Rigger.ManagedTypes.Implementations;
using Rigger.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Rigger.Tests
{
    public class Event
    {
        public bool IsCalled;
    }

    [Singleton]
    public class EventReceiver
    {
        public bool IsCalled { get; set; }



        public void CallMe(Event evt)
        {
            IsCalled = evt.IsCalled;
        }
        [OnEvent(typeof(Event))]
        public async Task CallMeAsync(Event evt)
        {
            IsCalled = evt.IsCalled;
            await Task.CompletedTask;
        }
    }
    public class EventTests
    {
        
        private ITestOutputHelper output;
        public EventTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestEvent()
        {
            var rig = new Rig();

            rig
                .Register<IEventRegistry, EventRegistry>()
                .Register<EventReceiver, EventReceiver>();
            
            var reg = rig.GetService<IEventRegistry>();

            var e = rig.GetService<EventReceiver>();

            reg.Fire(new Event {IsCalled = true});
            e.IsCalled.Should().BeTrue();
            
            reg.Fire(new Event {IsCalled = false});
            e.IsCalled.Should().BeFalse();
            
            await reg.FireAsync(new Event {IsCalled = true});
            e.IsCalled.Should().BeTrue();
        }

    }
}
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rigger.Attributes;
using Rigger.ManagedTypes;
using Xunit;
using Xunit.Abstractions;

namespace Rigger.Tests
{
    public class Event
    {
        public bool IsCalled;
    }
    public class AsyncEvent
    {
        public bool IsCalled;
    }

    [Singleton]
    public class EventReceiver
    {
        public bool IsCalled { get; set; }


        
        [OnEvent(typeof(Event))]
        public void CallMe(Event evt)
        {
            IsCalled = evt.IsCalled;
        }
        [OnEvent(typeof(AsyncEvent))]
        public async Task CallMeAsync(AsyncEvent evt)
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
        public void TestEvent()
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
        }
        
        [Fact]
        public async Task TestAsyncEvent()
        {
            var rig = new Rig();

            rig
                .Register<IEventRegistry, EventRegistry>()
                .Register<EventReceiver, EventReceiver>();
            
            var reg = rig.GetService<IEventRegistry>();

            var e = rig.GetService<EventReceiver>();

            await reg.FireAsync(new AsyncEvent {IsCalled = true});
            e.IsCalled.Should().BeTrue();

            await reg.FireAsync(new AsyncEvent {IsCalled = false});
            e.IsCalled.Should().BeFalse();

        }

    }
}
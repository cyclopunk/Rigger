using FluentAssertions;
using TheCommons.Forge.Events;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Traits.Attributes;
using Xunit;

namespace TheCommons.Forge.Tests
{
    // external class that will not be loaded automatically.
    public class ManagedTypeTest
    {

    }
    public class TestContainerEvents
    {
        
        [Singleton]
        public class EventTester
        {
            public int allEvents = 0;
            public int registerEvents = 0;
            public int ingotLoadEvents = 0;

            [OnEvent(Event = typeof(ContainerEvent))]
            public void OnAllEvents(ContainerEvent evt)
            {
                allEvents++;
            }
            [OnEvent(Event = typeof(TypeRegisteredEvent))]
            public void OnRegistrationEvents(TypeRegisteredEvent evt)
            {
                evt.Should().NotBe(null);

                registerEvents++;
            }
            [OnEvent(Event = typeof(IngotLoadedEvent))]
            public void OnIngotEvents(IngotLoadedEvent evt)
            {
                evt.Should().NotBe(null);
                
                evt.IngotType.Should().Be(typeof(TestContainerEvents));
                
                ingotLoadEvents++;
            }
        }

        public class EventContainerTester : ManagedTypes.Crucible
        {
            public EventContainerTester()
            {
                LoadIngot<TestContainerEvents>();
            }
        }

        class TestOne{}
        class TestTwo{}
        [Fact]
        public void ContainerEventsTests()
        {
            var app = new EventContainerTester();
            
            var eventTester = app.Get<EventTester>();

            app.LoadIngot<TestContainerEvents>();

            app.Register(RegistrationType.Ephemeral, typeof(TestOne));
            app.Register(RegistrationType.Ephemeral, typeof(TestTwo));

            app.Register<ManagedTypeTest, ManagedTypeTest>();
            
            app.Get<ManagedTypeTest>().Should().NotBeNull();

            eventTester.allEvents.Should().Be(4);
            eventTester.registerEvents.Should().Be(3); 
            eventTester.ingotLoadEvents.Should().Be(1); // service isn't started when the first ingot is loaded, so this should only catch one.
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rigger.Extensions;
using Rigger.Attributes;
using Rigger.ManagedTypes.Implementations;
using Rigger.ManagedTypes.Lightweight;

namespace Rigger.ManagedTypes
{
    /// <summary>
    /// Check an object for event listener methods and register those methods
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public class EventRegistry : IEventRegistry, IServiceAware
    {
        public Services Services { get; set; }
        
        /// <summary>
        /// Thread-safe storage of instance event registration.
        /// TODO registration maintenance
        /// </summary>
        private readonly ConcurrentBag<EventReceiver> _eventRegistry = new ConcurrentBag<EventReceiver>();
        public IEnumerable<EventReceiver> Register(object instance)
        {

            var methods = instance.GetType().MethodsWithAttribute<OnEventAttribute>();

            var registrations = methods.Map(o =>
            {
                OnEventAttribute attr = o.GetCustomAttribute<OnEventAttribute>();

                var registration = new EventReceiver
                {
                    Receiver = instance,
                    EventType = attr.Event,
                    Invoker = new ManagedMethodInvoker(instance.GetType(), o.Name)
                };

                _eventRegistry.Add(registration);

                return registration;
            });

            return registrations;
        }
        /// <summary>
        /// Helper method to fire an event to all receivers.
        /// </summary>
        public void Fire(object eventToFire)
        {
            _eventRegistry.ToList().FindAll(f =>
                {
                    var etype = f.EventType;
                    var ftype = eventToFire.GetType();

                    return f.EventType.IsAssignableFrom(eventToFire.GetType());
                })
                .ForEach(o =>
                {
                    o.Invoker.Invoke(o.Receiver, eventToFire);
                });
        }
        public async Task FireAsync(object eventToFire)
        {
            /*var tasks = _eventRegistry.ToList().FindAll(f => f.EventType == eventToFire.GetType())
                .Map(o => Task.Run( () =>
                {
                    if (o != null) return o.Invoker.Invoke(o.Receiver, eventToFire);
                }));

            await Task.WhenAll(tasks);*/
        }
    }
}
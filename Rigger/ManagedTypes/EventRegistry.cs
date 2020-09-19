using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rigger.Extensions;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes.Implementations;

namespace Rigger.ManagedTypes
{
    /// <summary>
    /// Check an object for event listener methods and register those methods
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public class EventRegistry : IEventRegistry, IServiceAware
    {
        public IServices Services { get; set; }

        /// <summary>
        /// Thread-safe storage of instance event registration.
        /// TODO registration maintenance
        /// </summary>
        private readonly ConcurrentDictionary<object, List<IEventReceiver>> _eventRegistry =
            new ConcurrentDictionary<object, List<IEventReceiver>>();
        private readonly ConcurrentDictionary<Type, List<IEventReceiver>> _typeCache = 
            new ConcurrentDictionary<Type, List<IEventReceiver>>();

        public IEnumerable<IEventReceiver> Register(object instance)
        {
            if (_eventRegistry.ContainsKey(instance))
            {
                return null;
            }

            var methods = instance.GetType().MethodsWithAttribute<OnEventAttribute>();

            var registrations = methods.Select(o =>
            {
                _eventRegistry.GetOrPut(instance, () => new List<IEventReceiver>());

                OnEventAttribute attr = o.GetCustomAttribute<OnEventAttribute>();
                _typeCache.GetOrPut( attr.Event, () => new List<IEventReceiver>());

                ManagedMethodInvoker invoker = new ManagedMethodInvoker(o);
                invoker.AddServices(Services);

                var registration = new EventReceiver
                {
                    Receiver = instance,
                    EventType = attr.Event,
                    Invoker = invoker
                };

                _eventRegistry[instance].Add(registration);
                _typeCache[attr.Event].Add(registration);

                return registration;
            });

            return registrations;
        }
        /// <summary>
        /// Helper method to fire an event to all receivers.
        /// </summary>
        public void Fire(object eventToFire)
        {
            if (!_typeCache.ContainsKey(eventToFire.GetType()))
            {
                return;
            }

            _typeCache[eventToFire.GetType()].ToList().Where(f =>
            {
                var etype = f.EventType;
                var ftype = eventToFire.GetType();

                return etype.IsAssignableFrom(ftype);
            }).ForEach(o =>
            {
                o.Invoker.Invoke(o.Receiver, eventToFire);
            });
        }
        public async Task FireAsync(object eventToFire)
        {
            var tasks = _typeCache[eventToFire.GetType()].ToList().Where(f => f.EventType == eventToFire.GetType())
                .Select(o => (Task) o?.Invoker?.Invoke(o.Receiver, eventToFire));

            await Task.WhenAll(tasks);
        }
    }
}
using System;
using TheCommons.Core.Reflection;

namespace TheCommons.Forge.ManagedTypes
{
    /// <summary>
    /// Registration class for a instance that will be
    /// an event receiver.
    /// </summary>
    public class EventReceiver : IRegistration
    {
        /// <summary>
        /// The type of class this registration will respond to
        /// </summary>
        public Type EventType { get; set; }
        /// <summary>
        /// Cached method invoker
        /// </summary>
        public IMethodInvoker Invoker { get; set; }
        /// <summary>
        /// Object receiver
        /// </summary>
        public object Receiver { get; set; }
    }
}
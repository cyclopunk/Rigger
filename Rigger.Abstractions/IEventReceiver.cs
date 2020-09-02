using System;
using Rigger.Reflection;

namespace Rigger.ManagedTypes
{
    /// <summary>
    /// Registration class for a instance that will be
    /// an event receiver.
    /// </summary>
    public interface IEventReceiver : IRegistration
    {
        /// <summary>
        /// The type of class this registration will respond to
        /// </summary>
        Type EventType { get; set; }
        /// <summary>
        /// Cached method invoker
        /// </summary>
        IMethodInvoker Invoker { get; set; }
        /// <summary>
        /// Object receiver
        /// </summary>
        object Receiver { get; set; }
    }
}
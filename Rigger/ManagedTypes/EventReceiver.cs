using System;
using Rigger.Reflection;

namespace Rigger.ManagedTypes
{
    public class EventReceiver : IEventReceiver
    {
        public Type EventType { get; set; }
        public IMethodInvoker Invoker { get; set; }
        public object Receiver { get; set; }
    }
}
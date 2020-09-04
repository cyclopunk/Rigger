using System;

namespace Rigger.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class OnEventAttribute : Attribute, ILifecycle
    {
        public Type Event { get; set; }

        public OnEventAttribute (Type evt)
        {
            this.Event = evt;
        }
    }
}
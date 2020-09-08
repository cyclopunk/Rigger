using System;

namespace Rigger.Injection
{
    public class CallSite
    {
        private readonly Type serviceType;

        public CallSite(Type serviceType, CallSiteType type)
        {
            this.serviceType = serviceType;
            this.Type = type;
        }

        public CallSiteType Type { get; }
        public Type LookupType { get; set; }
        public Type ImplementationType { get; set; }

        public bool HasSameLookupType(Type type)
        {
            return type == LookupType;
        }

        public override bool Equals(object obj)
        {
            if (obj is CallSite cs)
            {
                return cs.Type == this.Type && LookupType == cs.LookupType;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, serviceType);
        }
    }
}
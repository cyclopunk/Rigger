using System;
using Rigger.Attributes;

namespace Rigger.Injection
{
    public interface IServiceInstance : IDisposable
    {
        public IServices Services { get; set; }
        public Type LookupType { get; set; }
        public Type ServiceType { get; set; }
        public Type InstanceType { get; set; }
        public object Get();
    }
}
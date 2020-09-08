using System;

namespace Rigger.Injection
{
    public class ConditionalResolver : IServiceResolver
    {
        public IServices Services { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object Resolve()
        {
            return null;
        }
    }
}
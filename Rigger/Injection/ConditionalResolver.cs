using System;

namespace Rigger.Injection
{
    public class ConditionalResolver : ImplementationTypeResolver
    {
        private Type serviceType;
        private ExpressionTypeResolver resolver;

        public ConditionalResolver(IServices services, Type serviceType, ExpressionTypeResolver resolver) : base (services, resolver.ResolveType())
        {

            this.resolver = resolver;
            this.serviceType = serviceType;
        }

        public override object Resolve()
        {
            ImplementationType = resolver.ResolveType();
            
            return base.Resolve();
        }
    }
}
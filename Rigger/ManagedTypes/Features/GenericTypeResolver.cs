using System;
using TheCommons.Forge.ManagedTypes.Resolvers;

namespace TheCommons.Forge.ManagedTypes.Features
{
    public class GenericTypeResolver : ITypeResolver
    {
        private Type _type;

        public GenericTypeResolver(Type type)
        {
            this._type = type;
        }

        public Type ResolveType()
        {
            return _type;
        }
    }
}
using System;
using Rigger.ManagedTypes.Resolvers;

namespace Rigger.ManagedTypes.Features
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
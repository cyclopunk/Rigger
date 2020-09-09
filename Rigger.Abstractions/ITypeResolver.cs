using System;
using Rigger.Injection;

namespace Rigger.ManagedTypes.Resolvers
{
    public interface ITypeResolver : IServiceAware
    {
        Type ResolveType();
    }
}
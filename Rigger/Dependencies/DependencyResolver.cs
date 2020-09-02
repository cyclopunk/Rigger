using System;

namespace TheCommons.Forge.Dependencies
{
    /// <summary>
    /// Interface for a dependency resolver. This can be passed to a dependency tree
    /// to resolve the dependencies in the designated order.
    /// </summary>
    public interface IDependencyResolver
    {
        void Resolve(Type type);
    }
}
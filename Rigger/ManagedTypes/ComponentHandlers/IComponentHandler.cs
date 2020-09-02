using System;
using Rigger.ManagedTypes.Lightweight;

namespace Rigger.ManagedTypes.ComponentHandlers
{
    /// <summary>
    /// Interface for a component handler that handles a type with a certain attribute.
    /// </summary>
    /// <typeparam name="TAttributeType">The attribute type that can be found on the type this component handles</typeparam>
    public interface IComponentHandler <TAttributeType> : IServiceAware where TAttributeType : Attribute
    {
        public void HandleComponent(Type t);
    }
}
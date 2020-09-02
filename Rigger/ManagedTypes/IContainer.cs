using System;
using TheCommons.Forge.Exceptions;
using TheCommons.Forge.ManagedTypes.Lightweight;

namespace TheCommons.Forge.ManagedTypes
{
    /// <summary>
    /// Interface for an application container that manages instances of objects as well as services
    /// in an application.
    ///
    /// This interface provides the basic methods that allow registration and lookup of
    /// managed objects.
    /// </summary>
    public interface IContainer : IDisposable, IAsyncDisposable, IServiceProvider, IServiceAware
    {
        Services services { get; set; }
      /// <summary>
        /// Get a managed instance of the type provided
        /// </summary>
        /// <typeparam name="TInstance">The type to get an instance for</typeparam>
        /// <exception cref="ManagedTypeNotRegisteredException">Thrown if the type is not found within the container.</exception>
        /// <returns>An instance of the type provided.</returns>
        TInstance Get<TInstance>(params object[] constructorParams) where TInstance : class;
       /// <summary>
       /// Gets a managed instance of the type provided.
       /// </summary>
       /// <param name="type">The type to lookup in the container for an instance</param>
       /// <returns>An instance of that type.</returns>
       /// <exception cref="ManagedTypeNotRegisteredException">Thrown if the type is not found within the container.</exception>
       object Get(Type type, params object[] constructorParams);

       /// <summary>
       /// Register a singleton of a type.
       /// </summary>
       /// <param name="type"></param>
       /// <param name="instance"></param>
       /// <returns></returns>
       IContainer Register(Type type, object instance);
       /// <summary>
       /// Register a singleton with the provided type;
       /// </summary>
       /// <typeparam name="TType"></typeparam>
       /// <param name="instance"></param>
       /// <returns></returns>
       IContainer Register<TType>(object instance);
        /// <summary>
        /// Register a concrete type by another type. 
        /// </summary>
        /// <typeparam name="TInterface">The interface type of the implementation</typeparam>
        /// <typeparam name="TConcrete">A concrete (non-abstract, non-interface) type that will be used as the implementation</typeparam>
        /// <returns></returns>
        IContainer Register<TInterface, TConcrete>(ServiceLifecycle type=ServiceLifecycle.Singleton);

        /// <summary>
        /// Register and get a transient managed type.
        /// </summary>
        /// <typeparam name="TManagedType">The type to register and create an instance for</typeparam>
        /// <param name="constructorParams">Any parameters to pass to the constructor.</param>
        /// <returns></returns>
        TManagedType RegisterAndGet<TManagedType>(params object[] constructorParams) where TManagedType : class;
        
        /// <summary>
        /// Check to see if a type is managed.
        /// </summary>
        /// <typeparam name="TManagedType"</typeparam>
        /// <returns>True if the type is managed, false if not.</returns>
        public bool IsManaged<TManagedType>()
        {
            return IsManaged(typeof(TManagedType));
        }

        /// <summary>
        /// Check to see if a type is managed.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is managed, false if not.</returns>
        public bool IsManaged(Type type)
        {
            return Services.Get(type) != null;
        }

        /// <summary>
        /// Load an Ingot, a Type with nested Types that are marked with
        /// ManagedType attributes.
        /// </summary>
        /// <param name="type">The type to load.</param>
        public void LoadIngot(Type type);

        /// <summary>
        /// Load an Ingot, a Type with nested Types that are marked with
        /// ManagedType attributes.
        /// </summary>
        /// <typeparam name="TIngotType">The type to load</typeparam>
        public void LoadIngot<TIngotType>();
    }
}
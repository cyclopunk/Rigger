using System;

namespace TheCommons.Forge.Exceptions
{
    /// <summary>
    /// This exception is thrown when a managed type is not registered and is looked up
    /// either in the Inject process or when Get is called on
    /// the container.
    /// </summary>
    public class ManagedTypeNotRegisteredException : Exception
    {
        public ManagedTypeNotRegisteredException(string message) : base(message)
        {

        }
    }
}
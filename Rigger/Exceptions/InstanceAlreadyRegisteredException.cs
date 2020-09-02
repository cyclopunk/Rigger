using System;

namespace TheCommons.Forge.Lifecycle
{
    /// <summary>
    /// Exception that is thrown if an instance is already registered with the container.
    /// TODO possibly remove this class, instances that are already registered are now replaced or resolved on a conditional basis.  
    /// </summary>
    public class InstanceAlreadyRegisteredException : Exception
    {
        public InstanceAlreadyRegisteredException(string message) : base(message)
        {

        }
    }
}
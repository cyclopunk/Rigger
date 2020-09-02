using System;

namespace TheCommons.Forge.Exceptions
{
    public class CannotInstantiateException : Exception
    {
        public CannotInstantiateException(string message) : base(message)
        {

        }
    }
}
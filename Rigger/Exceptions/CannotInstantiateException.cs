using System;

namespace Rigger.Exceptions
{
    public class CannotInstantiateException : Exception
    {
        public CannotInstantiateException(string message) : base(message)
        {

        }
    }
}
using System;

namespace Rigger.Exceptions
{
    public class MethodValidationException : Exception
    {
        public MethodValidationException(string message) : base(message)
        {

        }
    }
}
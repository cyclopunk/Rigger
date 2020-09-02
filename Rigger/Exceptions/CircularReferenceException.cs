using System;

namespace Rigger.Exceptions
{
    public class CircularReferenceException  : Exception
    {
        public CircularReferenceException(Type type, Type reference) : base($"Circular reference of {reference} found in {type}")
        {

        }
    }
}
using System;

namespace TheCommons.Forge.Exceptions
{
    public class CircularReferenceException  : Exception
    {
        public CircularReferenceException(Type type, Type reference) : base($"Circular reference of {reference} found in {type}")
        {

        }
    }
}
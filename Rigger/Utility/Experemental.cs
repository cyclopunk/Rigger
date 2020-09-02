using System;

namespace Rigger.Utility
{
    /// <summary>
    /// "A" class for doing neat stuff with Generics and extension methods e.g.
    /// SomeType x = A<SomeType>.From.Json("{...}")
    /// which seems cleaner than
    /// SomeType y = typeof(SomeType).FromJson<SomeType>("{...}")
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class A<T> 
    {
        public static A<T> From =  new A<T>();

        public readonly T value;

        public A()
        {

        }
        public A(T value)
        {
            this.value = value;
        }
        public static Type Wrapped()
        {
            return typeof(T);
        }
    }

}
namespace Rigger.Reflection
{
    /// <summary>
    /// Interface to mark a class as a something that provides a value.
    /// </summary>
    public interface IValueGetter
    {
        public object GetValue(object source);
    }
    /// <summary>
    /// Typed version of a value getter.
    /// </summary>
    /// <typeparam name="T">The return type of the value</typeparam>
    public interface IValueGetter<T>
    {
        public T GetValue(object source);
    }
}
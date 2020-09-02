namespace Rigger.ValueConverters
{
    public interface IValueConverter
    {
        public object Convert(object from);
    }

    /// <summary>
    /// TInterface for value converters so they can be registered in the Forge DI container and
    /// can facilitate conversion between types (e.g. for configuration values which are most likely going
    /// to be strings).
    /// </summary>
    /// <typeparam name="T1">First type to convert from / to </typeparam>
    /// <typeparam name="T2">Second type to convert from / to</typeparam>
    public interface IValueConverter<T1, T2>
    {
        // TODO add bool to allow exceptions to be thrown on failed conversions
        /// <summary>
        /// Convert from TReturn to TFrom
        /// </summary>
        /// <param name="from">The T1 to convert from</param>
        /// <returns></returns>
        public T1 Convert(T2 from);
        public T2 Convert(T1 from);
    }
}
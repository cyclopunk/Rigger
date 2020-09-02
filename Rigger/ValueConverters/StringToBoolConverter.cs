namespace Rigger.ValueConverters
{
    /// <summary>
    /// Converter for booleans to string.
    /// </summary>
    public class StringToBoolConverter : IValueConverter<string, bool>, IValueConverter
    {
        public string Convert(bool from)
        {
            return from.ToString();
        }

        public bool Convert(string from)
        {
            try
            {
                return bool.Parse(from);
            }
            catch
            {
                return false;
            }
        }

        public object Convert(object from)
        {
            return from switch
            {
                bool b => Convert(b),
                string s => Convert(s),
                _ => false
            };
        }
    }
}
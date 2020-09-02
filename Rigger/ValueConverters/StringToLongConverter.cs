namespace Rigger.ValueConverters
{
    public class StringToLongConverter : IValueConverter<string, long>, IValueConverter
    {
        public string Convert(long from)
        {
            return from.ToString();
        }

        public long Convert(string from)
        {
            try
            {
                return long.Parse(from);
            }
            catch
            {
                return 0L;
            }
        }
        public object Convert(object from)
        {
            return from switch
            {
                long d => Convert(d),
                string s => Convert(s),
                _ => 0L
            };
        }
    }
}
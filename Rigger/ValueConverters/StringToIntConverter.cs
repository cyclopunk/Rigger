namespace Rigger.ValueConverters
{
    public class StringToIntConverter : IValueConverter<string, int>, IValueConverter
    {
        public string Convert(int from)
        {
            return from.ToString();
        }

        public int Convert(string from)
        {
            try
            {
                return int.Parse(from);
            }
            catch
            {
                return 0;
            }
        }
        public object Convert(object from)
        {
            return from switch
            {
                int i => Convert(i),
                string s => Convert(s),
                _ => 0
            };
        }
    }
}
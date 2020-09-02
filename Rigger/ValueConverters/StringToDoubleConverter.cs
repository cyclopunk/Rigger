namespace Rigger.ValueConverters
{
    public class StringToDoubleConverter : IValueConverter<string, double>, IValueConverter
    {
        public string Convert(double from)
        {
            return from.ToString();
        }

        public double Convert(string from)
        {
            try
            {
                return double.Parse(from);
            }
            catch
            {
                return 0.0;
            }
        }
        public object Convert(object from)
        {
            return from switch
            {
                double d => Convert(d),
                string s => Convert(s),
                _ => 0.0
            };
        }
    }
}
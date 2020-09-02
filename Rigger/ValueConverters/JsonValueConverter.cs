using Newtonsoft.Json;

namespace Rigger.ValueConverters
{
    // IValueConverter wrapper for Json.Net
    public class JsonValueConverter<TObject> : IValueConverter<string, TObject>
    {
        public string Convert(TObject from)
        {
            return JsonConvert.SerializeObject(from);
        }

        public TObject Convert(string from)
        {
            return JsonConvert.DeserializeObject<TObject>(from);
        }
    }
}
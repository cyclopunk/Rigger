using System.Collections.Generic;
using System.Linq;
using Rigger.Configuration;

namespace Drone.Configuration.Sources
{
    /// <summary>
    /// Simple map based configuration source.
    /// </summary>
    public class MapConfigurationSource : IConfigurationSource
    {
        public int Priority { get; set; } = 100;

        public Dictionary<string, object> BackingMap { get; set; } = new Dictionary<string, object>();

        public IEnumerable<string> GetAllKeys()
        {
            return BackingMap.Keys.ToList();
        }

        public Dictionary<string, object> Fetch()
        {
            return BackingMap;
        }
    }
}
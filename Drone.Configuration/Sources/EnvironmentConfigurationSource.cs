using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Rigger.Extensions;
using IConfigurationSource = Rigger.Configuration.IConfigurationSource;

namespace Drone.Configuration.Sources
{
    /// <summary>
    /// Configuration source that loads all environment variables into the configuration.
    /// </summary>
    public class EnvironmentConfigurationSource : IConfigurationSource
    {
        public int Priority { get; set; }
        public IEnumerable<string> GetAllKeys()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            
            var configuration = builder.Build();
            
            return configuration.AsEnumerable().Select( o => o.Key );
        }

        public Dictionary<string, object> Fetch()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            
            var dictionary = new Dictionary<string, object>();

            configuration
                .AsEnumerable()
                .ForEach(k => dictionary.Add(k.Key, k.Value));

            return dictionary;
        }
    }
}
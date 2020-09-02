﻿using System.Collections.Generic;
using System.Linq;
using Rigger.Extensions;

namespace Rigger.Configuration.Sources
{
    public class DatabaseConfigurationSource : IConfigurationSource
    {
        private ConfigurationEntityContext configurationContext;

        public DatabaseConfigurationSource(ConfigurationEntityContext configurationContext)
        {
            this.configurationContext = configurationContext;
        }


        public int Priority { get; set; }
        public IEnumerable<string> GetAllKeys()
        {
            return configurationContext.ConfigurationEntity.Select(o => o.Name).ToList();
        }

        public Dictionary<string, object> Fetch()
        {
            var dictionary = new Dictionary<string, object>();

            configurationContext.ConfigurationEntity.ForEach(o => dictionary.Add(o.Name, o.Value));

            return dictionary;
        }
    }
}
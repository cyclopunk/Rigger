using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Rigger.Attributes;
using Rigger.Configuration.Validation;
using Rigger.Extensions;

namespace Rigger.Configuration
{

    /// <summary>
    /// Service for loading configuration and getting values from the configuration sources.
    /// We assume configuration values are read-only and should only be changed in their corresponding sources.
    /// </summary>
    public class DefaultConfigurationService : IConfigurationService
    {
        [Autowire] private IEnumerable<IConfigurationValidator> validators;

        [Autowire] private IEnumerable<IConfigurationSource> sources;
        // immutable / volatile to provide thread safety
        private volatile ImmutableDictionary<string, object> _compiledConfiguration = null;

        /// <summary>
        /// Fetch the configuration from all of the sources. This has the side-effect of clearing out
        /// all of the configuration values.
        /// </summary>
        public void FetchConfiguration()
        {
            // clear the current configuration
            Dictionary<string, object> compiledConfiguration = new Dictionary<string, object>();
            // load the new configuration.
            sources.OrderBy(src => src.Priority)
                .ForEach(s => s.Fetch().ForEach(x =>
                {
                    compiledConfiguration.Remove(x.Key);
                    compiledConfiguration.Add(x.Key, x.Value);
                }));

            _compiledConfiguration = compiledConfiguration.ToImmutableDictionary();
        }

        /// <summary>
        /// Validate the configuration using all supplied Configuration Validators.
        /// </summary>
        /// <returns></returns>
        public virtual bool ValidateConfiguration()
        {
            return validators?.All(o => o.IsValid(sources.ToArray())) == true;
        }

        public IEnumerable<string> GetKeys()
        {
            return _compiledConfiguration.Keys;
        }

        public object Get(string key)
        {
            _compiledConfiguration.TryGetValue(key, out var value);

            return value;
        }

        /// <summary>
        /// Get a value from the configuration storage.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public TValue Get<TValue>(string key, TValue @default)
        {
            if (_compiledConfiguration == null)
            {
                throw new Exception("Configuration has not yet been initialized, please call add Configuration sources and call FetchConfiguration() first.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Cannot look up a null configuration key.");
            }

            if (!_compiledConfiguration.ContainsKey(key))
            {
                return @default;
            }

            return (TValue) _compiledConfiguration[key];
        }

        /**
         * Add a configuration source and provide method chaining.
         */
        public IConfigurationService AddSource(IConfigurationSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source cannot be null while adding a configuration source.");
            }

            sources = sources.Append(source);

            FetchConfiguration();

            return this;
        }
    }
}
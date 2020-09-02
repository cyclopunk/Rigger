using System;

namespace Rigger.Configuration
{
    /**
     * Basic entity for storing configuration values.
     */
    public class ConfigurationEntity
    {

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Value { get; set; }
        public string ValueContentType { get; set; }
        public string ValidatorPattern { get; set; }
    }
}
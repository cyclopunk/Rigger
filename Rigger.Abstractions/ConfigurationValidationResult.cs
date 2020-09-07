using System;

namespace Rigger.Configuration.Validation
{
    public enum ValidationLevel
    {
        Info,
        Warning,
        Fatal
    }
    public class ConfigurationValidationResult
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public ValidationLevel Level { get; set; }
    }
}
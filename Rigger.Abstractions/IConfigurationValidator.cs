using System.Collections.Generic;

namespace Rigger.Configuration.Validation
{
    public interface IConfigurationValidator
    {
        bool IsValid(params IConfigurationSource[] sources);
        IEnumerable<ConfigurationValidationResult> Validate(params IConfigurationSource[] sources);
    }
}
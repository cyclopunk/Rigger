using System.Collections.Generic;
using System.Linq;

namespace Rigger.Configuration.Validation
{
  
    /// <summary>
    /// Configuration Validator abstract class.
    /// </summary>
    public abstract class AbstractConfigurationValidator : IConfigurationValidator
    {
        public virtual bool IsValid(params IConfigurationSource[] sources)
        {
            IEnumerable<ConfigurationValidationResult> validations = Validate(sources);

            return validations == null || !validations.Any(); // true if validation results are null or empty
        }

        public abstract IEnumerable<ConfigurationValidationResult> Validate(params IConfigurationSource[] sources);
    }
}
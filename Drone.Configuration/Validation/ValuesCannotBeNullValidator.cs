using System;
using System.Collections.Generic;
using System.Linq;
using Rigger.Configuration;
using Rigger.Configuration.Validation;
using Rigger.Extensions;

namespace Drone.Configuration.Validation
{
    /**
     * Checks that keys which are passed to the constructor of this validator
     * are not null in the configuration sources passed to Validate.
     */
    public class ValuesCannotBeNullValidator : AbstractConfigurationValidator
    {
        private readonly IEnumerable<string> _keysToCheck;

        public ValuesCannotBeNullValidator(params string[] keysToCheck)
        {
            _keysToCheck = keysToCheck;
        }


        public override IEnumerable<ConfigurationValidationResult> Validate(params IConfigurationSource[] sources)
        {
            List<ConfigurationValidationResult> validationResults = new List<ConfigurationValidationResult>();

            IEnumerable<KeyValuePair<string, object>> keyValuePair = sources
                .Select(s => s.Fetch().Where(c => _keysToCheck.Contains(c.Key) && c.Value == null)) // get all key value pairs with null values that have been passed to this validator.
                .Combine(); // combine them into one list.


            return keyValuePair.Select(o => new ConfigurationValidationResult
            {
                Exception = new Exception( $"[ValuesCannotBeNullValidator] Configuration setting {o.Key} is null and should not be."),
                Level = ValidationLevel.Fatal,
                Message = $"[ValuesCannotBeNullValidator] Configuration setting {o.Key} is null and should not be.",
                Name = o.Key,
                Value = null
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rigger.Configuration;
using Rigger.Configuration.Validation;

namespace Drone.Configuration.Validation
{
    /**
     * Validator for a specific key to match a regular expression passed by valueMatcher.
     * TODO Fail fast on pattern problem.
     */
    public class ValueMatchesValidator : AbstractConfigurationValidator
    {
        private readonly string _key;
        private readonly string _valueMatcher;

        public ValueMatchesValidator(string key, string valueMatcher)
        {
            _valueMatcher = valueMatcher;
            _key = key;
        }

        public override IEnumerable<ConfigurationValidationResult> Validate(params IConfigurationSource[] sources)
        {
            object value = sources.OrderBy(o => o.Priority).LastOrDefault()?.Fetch()[_key];

            if (value != null)
            {
                try
                {
                    Regex rx = new Regex(_valueMatcher);

                    if (rx.IsMatch(value.ToString()))
                    {
                        return null;
                    }
                    // if we don't match, return a negative result.
                    return new[]
                    {
                        new ConfigurationValidationResult()
                        {
                            Exception = null,
                            Level = ValidationLevel.Warning,
                            Message = $"Value for {_key} did not match validator {_valueMatcher}",
                            Name = _key,
                            Value = value
                        }
                    };
                }
                catch (Exception ex)
                {
                    return new[]
                    {
                        new ConfigurationValidationResult()
                        {
                            Exception = ex,
                            Level = ValidationLevel.Warning,
                            Message = $"Regular expression {_valueMatcher} did not compile because {ex.Message}",
                            Name = _key,
                            Value = value
                        }
                    };
                }
            }

            return null;
        }

        // TODO Implement
        public override bool IsValid(params IConfigurationSource[] sources)
        {
            return true;
        }

    }
}
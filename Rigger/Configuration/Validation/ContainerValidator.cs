using System;
using System.Collections.Generic;
using System.Linq;
using Rigger.Extensions;

namespace Rigger.Configuration.Validation
{
    /**
     * Validator that can contain other validators.
     */
    public class ContainerValidator : AbstractConfigurationValidator
    {
        private readonly IEnumerable<IConfigurationValidator> _otherValidators;

        public ContainerValidator(IEnumerable<IConfigurationValidator> otherValidators)
        {
            this._otherValidators = otherValidators;
        }

        public override IEnumerable<ConfigurationValidationResult> Validate(params IConfigurationSource[] sources)
        {
            if (sources.Length == 0)
            {
                throw new ArgumentException("Configuration sources cannot be length 0 when passed to Validate", nameof(sources));
            }

            return _otherValidators.Select(o => Validate(sources)).Combine();
        }
    }
}
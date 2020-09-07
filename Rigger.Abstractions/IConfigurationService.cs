using System;
using System.Collections;
using System.Collections.Generic;

namespace Rigger.Configuration
{
    /// <summary>
    /// Interface for a configuration service. Configuration services will load configuration values from
    /// different sources and provide them through the Get method.
    ///
    /// </summary>
    public interface IConfigurationService
    {
        bool ValidateConfiguration();

        IEnumerable<string> GetKeys();
        object Get(string key);
        TValue Get<TValue>(string key, TValue _default);
        IConfigurationService AddSource(IConfigurationSource source);
    }
}
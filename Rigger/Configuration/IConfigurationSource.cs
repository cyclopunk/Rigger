using System.Collections.Generic;
using System.Net.Mail;

namespace Rigger.Configuration
{
    /// <summary>
    /// Interface for classes that are configuration sources.
    /// </summary>
    public interface IConfigurationSource
    {
        int Priority { get; set; }
        IEnumerable<string> GetAllKeys();
        Dictionary<string, object> Fetch();
    }
}
using System;
using Microsoft.Extensions.Logging;
using Rigger;
using Rigger.Attributes;
using Rigger.Injection;
using Rigger.ManagedTypes;

namespace Drone.Bootstrap
{
    /// <summary>
    /// Module that sets up a default environment for Rigger.
    /// </summary>
    [Module]
    public class DefaultModule
    {
        public DefaultModule(Services services)
        {
            services.Add(typeof(ILogger<>), typeof(ConsoleLogger));
        }
    }
}

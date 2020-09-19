using Drone.Configuration.Sources;
using Rigger.Attributes;
using Rigger.Configuration;
using Rigger.Injection;
using Rigger.Injection.Defaults;
using ServiceLifetime = Rigger.Injection.ServiceLifetime;

namespace Drone.Configuration
{
    /// <summary>
    /// Module that sets up a default setup for a rigged application.
    ///
    /// This default can be changed by other modules in the component scanning paths.
    /// </summary>
    [Module(Priority = -1)]
    public class DefaultModule
    {
        public DefaultModule(IServices services)
        {
            services
                .Add<IConfigurationService, DefaultConfigurationService>(ServiceLifetime.Singleton)
                .Add<IConfigurationSource, EnvironmentConfigurationSource>(ServiceLifetime.Singleton)
                .Add<IConfigurationSource, AppSettingsConfigurationSource>(ServiceLifetime.Singleton) // default config
                .Add<IValueInjector, DefaultValueInjector>();
        }
    }
}

using System;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Extensions.Logging;
using Rigger.Scrubbers;

namespace Rigger.Implementations.Azure
{
    public abstract class AbstractAzureLogger : AbstractLoggingService
    {
        private const string LogName = "seriLogger";

        protected AbstractAzureLogger()
        {
            ILogger seriLogger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.FromLogContext()
                .Enrich.WithCallStack()
                .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                .CreateLogger();

            ConfigureLogger(seriLogger);
        }

        protected AbstractAzureLogger(Type type)
        {
            ILogger seriLogger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.FromLogContext()
                .Enrich.WithCallStack()
                .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                .CreateLogger()
                .ForContext(type);

            ConfigureLogger(seriLogger);
        }

        private void ConfigureLogger(ILogger seriLogger)
        {
            SerilogLoggerProvider serilogLoggerProvider = new SerilogLoggerProvider(seriLogger);

            base.LogScrubber = new NewLineLogScrubber();
            Logger = serilogLoggerProvider.CreateLogger(LogName);
        }
    }
}

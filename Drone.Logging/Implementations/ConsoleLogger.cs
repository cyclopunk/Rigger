using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Rigger
{
    /**
     * Logger that will log to the console.
     */
    public class ConsoleLogger : AbstractLoggingService , ILoggerFactory
    {
        private const string LogName = "seriLogger";
        
        public ConsoleLogger()
        {
            ILogger seriLogger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.FromLogContext()
                .Enrich.WithCallStack()
                .WriteTo.Console()
                .CreateLogger();

            LoggerFactory = new SerilogLoggerFactory(seriLogger, false);
        }

        public void Dispose()
        {
            LoggerFactory.Dispose();
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return LoggerFactory.CreateLogger(categoryName);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            LoggerFactory.AddProvider(provider);
        }
    }
}
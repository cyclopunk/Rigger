using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Rigger
{
    /**
     * Logger that will log to the console.
     */
    public class ConsoleLogger : AbstractLoggingService 
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

    }
}
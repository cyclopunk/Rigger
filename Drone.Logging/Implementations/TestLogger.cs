using Serilog;
using Serilog.Extensions.Logging;
using Rigger.Sinks;
using Xunit.Abstractions;

namespace Rigger.Implementations
{
    public class TestLogger : AbstractLoggingService
    {
        private const string LogName = "seriLogger";

        public TestLogger(ITestOutputHelper output)
        {
            var jsonListSink = new JsonListSink();
            ILogger seriLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Enrich.WithCallStack()
                .WriteTo.Sink(jsonListSink)
                .WriteTo.TestOutput(output, outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{Properties}")
                .CreateLogger();

            
            LoggerFactory = new SerilogLoggerFactory(seriLogger, false);

            SerilogLoggerProvider serilogLoggerProvider = new SerilogLoggerProvider(seriLogger);

            Logger = serilogLoggerProvider.CreateLogger(LogName);

        }
    }
}
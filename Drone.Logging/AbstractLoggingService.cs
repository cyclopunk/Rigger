using System;
using Microsoft.Extensions.Logging;
using Rigger.Scrubbers;

namespace Rigger
{
    /**
     * Abstracting log service that implements Serilog as an ILogger.
     */
    public abstract class AbstractLoggingService
    {
        public LogScrubber LogScrubber { get; set; } = new DefaultLogScrubber();
        
        public ILoggerFactory LoggerFactory { get; protected set; }

        public ILogger Logger
        {
            get => LoggerFactory.CreateLogger<AbstractLoggingService>();
            protected set {}
        }

        public ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        protected AbstractLoggingService(LogScrubber logScrubber)
        {
            LogScrubber = logScrubber ?? throw new ArgumentNullException(nameof(logScrubber), "Log scrubber is null during initialization.");
        }

        protected AbstractLoggingService()
        {
        }
        
        public void Log(LogEntry entry)
        {
            string scrubbedMessage = LogScrubber.Scrub(entry.Message);
            object[] scrubbedArguments = ScrubStoredValues(entry.Arguments);

            switch (entry.Severity)
            {
                case LoggingEventType.Fatal:
                    Log(LogLevel.Critical, scrubbedMessage, null, scrubbedArguments);
                    break;
                case LoggingEventType.Debug:
                    Log(LogLevel.Debug, scrubbedMessage, null, scrubbedArguments);
                    break;
                case LoggingEventType.Error:
                    Log(LogLevel.Error, scrubbedMessage, entry.Exception, scrubbedArguments);
                    break;
                case LoggingEventType.Information:
                    Log(LogLevel.Information, scrubbedMessage, entry.Exception, scrubbedArguments);
                    break;
                case LoggingEventType.Warning:
                    Log(LogLevel.Warning, scrubbedMessage, entry.Exception, scrubbedArguments);
                    break;
                default:
                    Log(LogLevel.Warning, $"Encountered unknown log level {entry.Severity}, writing out as Info.");
                    Log(LogLevel.Information, scrubbedMessage, entry.Exception, scrubbedArguments);
                    break;
            }
        }

        public void Debug(string message, params object[] args)
        {
            GenerateLogEntry(LoggingEventType.Debug, null, message, args);
        }

        public void Information(string message, params object[] args)
        {
            GenerateLogEntry(LoggingEventType.Information, null, message, args);
        }

        public void Warning(string message, params object[] args)
        {
            GenerateLogEntry(LoggingEventType.Warning, null, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Error(null, message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            GenerateLogEntry(LoggingEventType.Error, exception, message, args);
        }

        private void GenerateLogEntry(
            LoggingEventType logLevel,
            Exception exception,
            string message,
            params object[] args
        ) {
            Log(new LogEntry(logLevel, message, exception, args));
        }

        private void Log(
            LogLevel logLevel,
            string message,
            Exception exception = null,
            params object[] args 
        ) {
            Logger.Log(logLevel, exception, message, args);
        }

        private object[] ScrubStoredValues(object[] arguments)
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is string)
                {
                    arguments[i] = LogScrubber.Scrub(arguments[i].ToString());
                }
            }

            return arguments;
        } 
    }
}
using System;

namespace Rigger
{
    public enum LoggingEventType
    {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    };

    public class LogEntry
    {
        public LoggingEventType Severity { get; }

        public string Message { get; }

        public object[] Arguments { get; }

        public Exception Exception { get; }

        public LogEntry(
            LoggingEventType severity,
            string message,
            Exception exception = null,
            params object[] args
        )
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message == string.Empty)
            {
                throw new ArgumentException("empty", "message");
            }

            Severity = severity;
            Message = message;
            Exception = exception;
            Arguments = args;
        }
    }
}
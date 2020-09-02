using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Rigger
{
    public class CallStackEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            int skip = 3;

            while (true)
            {
                StackFrame stack = new StackFrame(skip);

                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("MethodInfo", new ScalarValue("<unknown method>")));
                    return;
                }

                MethodBase method = stack.GetMethod();

                if (method.DeclaringType != null && method.DeclaringType.Assembly != typeof(Log).Assembly && method.DeclaringType != typeof(AbstractLoggingService))
                {
                    string methodInfo = $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.FullName))})";
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("MethodInfo", new ScalarValue(methodInfo)));
                }

                skip++;
            }
        }
    }

    public static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCallStack(this LoggerEnrichmentConfiguration enrichmentConfiguration)
            => enrichmentConfiguration.With<CallStackEnricher>();
    }
}
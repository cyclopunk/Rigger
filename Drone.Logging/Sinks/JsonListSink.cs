using System.Collections.Generic;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Rigger.Sinks
{
    /**
     * Sink that will store all log messages to a list in JSON. Should only be used for testing
     * so logging can be tested as well.
     */
    public class JsonListSink : ILogEventSink
    {
       
        public List<string> logs = new List<string>();
        public void Emit(LogEvent logEvent)
        {

            StringWriter output = new StringWriter();

            new JsonFormatter().Format(logEvent, output);

            logs.Add(output.ToString());
        }
    }
}
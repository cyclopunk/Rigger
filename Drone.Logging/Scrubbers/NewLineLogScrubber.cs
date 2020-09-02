using System;

namespace Rigger.Scrubbers
{
    public class NewLineLogScrubber : LogScrubber
    {
        private readonly string _replaceValue = string.Empty;

        public NewLineLogScrubber()
        {

        }

        public NewLineLogScrubber(LogScrubber logScrubber)
            : base(logScrubber)
        {

        }

        public override string Scrub(string logLing)
        {
            logLing = base.Scrub(logLing);
            return logLing.Replace(Environment.NewLine, _replaceValue);
        }
    }
}

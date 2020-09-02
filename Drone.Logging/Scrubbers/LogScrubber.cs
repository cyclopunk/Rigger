namespace Rigger.Scrubbers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class LogScrubber
    {
        private readonly LogScrubber _scrubber;

        protected LogScrubber()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scrubber"></param>
        protected LogScrubber(LogScrubber scrubber)
        {
            _scrubber = scrubber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLing"></param>
        /// <returns></returns>
        public virtual string Scrub(string logLing)
            => _scrubber != null ? _scrubber.Scrub(logLing) : logLing;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Rigger.Extensions;
using Rigger.Attributes;

namespace Rigger.Utility
{
    [Managed]
    public class Profiler : IDisposable
    {

        [Autowire] private ILogger<Profiler> logger;
        public string Name { get; set; }
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public Dictionary<string, long> Tracking = new Dictionary<string, long>();

        public long TotalTime { get; } = 0L;
        
        private long _lastTrack;
        public Profiler(string name)
        {
            this.Name = name;
            Stopwatch.Start();
        }
        public void Track(string thingToTrack)
        {
            Tracking[thingToTrack] = Stopwatch.ElapsedMilliseconds - _lastTrack;
            
            _lastTrack = Stopwatch.ElapsedMilliseconds;
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            logger.LogInformation($"[Profile] {Name} took {Stopwatch.ElapsedMilliseconds}ms total");
            Tracking.ForEach(o => logger.LogInformation($"[Profile({Name})] {o.Key} took {o.Value}ms"));
        }
    }
}
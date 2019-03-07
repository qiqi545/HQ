using System;
using System.Diagnostics;

namespace HQ.Extensions.Metrics
{
    /// <summary>
    /// Wraps a timing closure with additional data. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimerHandle<T>
    {
        private readonly Stopwatch _stopwatch;
        
        public DateTimeOffset? StartedAt { get; private set; }
        public DateTimeOffset? StoppedAt { get; private set; }
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public bool IsStarted => StartedAt.HasValue;
        public bool IsStopped => StoppedAt.HasValue;
        public bool IsRunning => IsStarted && !IsStopped;

        public T Value { get; set; }

        public TimerHandle(Stopwatch stopwatch)
        {
            _stopwatch = stopwatch;
            StartedAt = default;
            StoppedAt = default;
            Value = default;
        }
        
        public void Start()
        {
            _stopwatch.Start();
            StartedAt = DateTimeOffset.UtcNow;
        }

        public void Stop()
        {
            _stopwatch.Stop();
            StoppedAt = DateTimeOffset.UtcNow;
        }
        
        public static implicit operator T(TimerHandle<T> h)
        {
            return h.Value;
        }
    }
}

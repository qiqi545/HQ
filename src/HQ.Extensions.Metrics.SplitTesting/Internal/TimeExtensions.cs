using System;

namespace HQ.Extensions.Metrics.SplitTesting.Internal
{
    internal static class TimeExtensions
    {
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static DateTimeOffset FromUnixTime(this long seconds)
        {
            return Epoch.AddSeconds(seconds).ToLocalTime();
        }

        public static long ToUnixTime(this DateTimeOffset dateTime)
        {
            var timeSpan = dateTime - Epoch;
            var timestamp = (long)timeSpan.TotalSeconds;

            return timestamp;
        }
    }
}

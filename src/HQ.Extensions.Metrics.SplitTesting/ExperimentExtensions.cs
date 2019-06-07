using System;
using System.Collections.Concurrent;
using System.Linq;
using HQ.Extensions.Metrics.SplitTesting.Internal;

namespace HQ.Extensions.Metrics.SplitTesting
{
    public static class ExperimentExtensions
    {
        static ExperimentExtensions()
        {
            SampleStore.Samples = new ConcurrentBag<Sample>();
        }

        private const string Separator = "__";
        internal const string Header = "__m__track__";

        public static void TrackExperiment(this IMetricsHost host, string metric, int increment = 1)
        {
            if (increment <= 0) return;

            var now = DateTimeOffset.UtcNow;
            var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset).ToUnixTime();
            
            var count = host.Counter(typeof(ExperimentExtensions), InternalMetric(metric, today)).Increment(increment);

            SampleStore.Samples.TryAdd(new Sample
            {
                Metric = metric,
                Value = count,
                SampledAt = now
            });

            // Track conversions for any registered experiments
            var experiments = Experiments.All.Values.Where(e => e.HasMetric(metric));
            foreach (var experiment in experiments)
            {
                experiment.Conversions[experiment.Alternative]++;
                experiment.CurrentParticipant.Converted = now;
            }
        }

        private static string InternalMetric(string tag, long timestamp)
        {
            return string.Concat(Header, tag, Separator, timestamp);
        }
    }
}

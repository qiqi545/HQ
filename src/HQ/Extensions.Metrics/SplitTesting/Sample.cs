using System;

namespace HQ.Extensions.Metrics.SplitTesting
{
    public class Sample
    {
        public string Metric { get; set; }
        public long Value { get; set; }
        public DateTimeOffset SampledAt { get; set; }
    }
}

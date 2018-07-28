using System.Collections.Generic;
using cadence.Core;
using Xunit;

namespace cadence.tests.Core
{
    public class GaugeTests
    {
        [Fact]
        public void Can_gauge_scalar_value()
        {
            var queue = new Queue<int>();
            var gauge = new GaugeMetric<int>(() => queue.Count);

            queue.Enqueue(5);
            Assert.Equal(1, gauge.Value);

            queue.Enqueue(6);
            queue.Dequeue();
            Assert.Equal(1, gauge.Value);

            queue.Dequeue();
            Assert.Equal(0, gauge.Value);
        }

        [Fact]
        public void Can_use_gauge_metric()
        {
            var queue = new Queue<int>();
            var gauge = Metrics.Gauge(typeof(GaugeTests), "Can_use_gauge_metric", () => queue.Count);
            queue.Enqueue(5);
            Assert.Equal(1, gauge.Value);
        }
    }
}

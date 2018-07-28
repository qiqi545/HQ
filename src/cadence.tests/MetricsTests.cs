using cadence.Core;
using cadence.tests.Core;
using Xunit;

namespace cadence.tests
{
    public class MetricsTests : IClassFixture<MetricsFixture>
    {
        [Fact]
        public void Can_get_same_metric_when_metric_exists()
        {
            var counter = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.NotNull(counter);

            var same = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.Same(counter, same);
        }

		[Fact]
		public void Can_get_all_registered_metrics()
        {
            var counter = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.NotNull(counter);

            var same = Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.NotNull(same);

            Assert.Equal(1, Metrics.All.Count);
        }

		[Fact]
		public void Can_get_all_registered_metrics_as_readonly()
        {
            var all = Metrics.All.Count;

            Assert.Equal(0, all);

            Metrics.All.Add(new MetricName(typeof(CounterTests), "No way this is going to get added"), new CounterMetric());

            Assert.Equal(0, all);
        }

		[Fact]
		public void Can_get_all_registered_metrics_as_readonly_and_immutable()
        {
            Assert.Equal(0, Metrics.All.Count);

            var name = new MetricName(typeof (CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

            Metrics.Counter(typeof (CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

            Assert.Equal(1, Metrics.All.Count);

            var value = Metrics.All[name];

            Assert.NotNull(value);

            ((CounterMetric)value).Increment();

            Assert.Equal(0, ((CounterMetric)Metrics.All[name]).Count);
        }
    }
}

using cadence.Core;
using cadence.tests.Core;
using Xunit;

namespace cadence.tests
{
    public class MetricsTests : IClassFixture<MetricsFixture>
    {
	    readonly MetricsFixture _fixture;

	    public MetricsTests(MetricsFixture fixture)
	    {
		    _fixture = fixture;
	    }

		[Fact]
        public void Can_get_same_metric_when_metric_exists()
        {
            var counter = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.NotNull(counter);

            var same = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.Same(counter, same);
        }

		[Fact]
		public void Can_get_all_registered_metrics()
        {
            var counter = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_all_registered_metrics");
            Assert.NotNull(counter);

            var same = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_all_registered_metrics");
            Assert.NotNull(same);

            Assert.Equal(1, _fixture.Metrics.All.Count);
        }

		[Fact]
		public void Can_get_all_registered_metrics_as_readonly()
        {
            var all = _fixture.Metrics.All.Count;

            Assert.Equal(0, all);

            _fixture.Metrics.All.Add(new MetricName(typeof(CounterTests), "No way this is going to get added"), new CounterMetric());

            Assert.Equal(0, all);
        }

		[Fact]
		public void Can_get_all_registered_metrics_as_readonly_and_immutable()
        {
	        using (var metrics = new MetricsFixture())
	        {
		        Assert.Equal(0, metrics.Metrics.All.Count);

		        var name = new MetricName(typeof(CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

		        metrics.Metrics.Counter(typeof(CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

		        Assert.Equal(1, metrics.Metrics.All.Count);

		        var value = metrics.Metrics.All[name];

		        Assert.NotNull(value);

		        ((CounterMetric)value).Increment();

		        Assert.Equal(0, ((CounterMetric)metrics.Metrics.All[name]).Count);
			}
				
        }
    }
}

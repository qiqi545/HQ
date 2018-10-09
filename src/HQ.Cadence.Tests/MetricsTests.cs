// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Cadence.Tests.Core;
using Xunit;

namespace HQ.Cadence.Tests
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

            Assert.Equal(1, _fixture.Metrics.AsReadOnly.Count);
        }

		[Fact]
		public void Can_get_all_registered_metrics_as_readonly_and_immutable()
        {
	        using (var metrics = new MetricsFixture())
	        {
		        Assert.Equal(0, metrics.Metrics.AsReadOnly.Count);

		        var name = new MetricName(typeof(CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

		        metrics.Metrics.Counter(typeof(CounterTests), "Can_get_all_registered_metrics_as_readonly_and_immutable");

		        Assert.Equal(1, metrics.Metrics.AsReadOnly.Count);

		        var value = metrics.Metrics.AsReadOnly[name];

		        Assert.NotNull(value);

		        ((CounterMetric)value).Increment();

		        Assert.Equal(0, ((CounterMetric)metrics.Metrics.AsReadOnly[name]).Count);
			}	
        }
    }
}

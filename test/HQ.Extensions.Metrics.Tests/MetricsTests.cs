#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using Xunit;

namespace HQ.Extensions.Metrics.Tests
{
    public class MetricsTests : IClassFixture<MetricsFixture>
    {
        public MetricsTests(MetricsFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly MetricsFixture _fixture;

        [Fact]
        public void Can_get_all_registered_metrics()
        {
            var counter = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_all_registered_metrics");
            Assert.NotNull(counter);

            var same = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_all_registered_metrics");
            Assert.NotNull(same);

            Assert.Equal(1, _fixture.Metrics.GetSample().Count);
        }

        [Fact]
        public void Can_get_all_registered_metrics_as_readonly_and_immutable()
        {
            using (var metrics = new MetricsFixture())
            {
                Assert.Equal(0, metrics.Metrics.GetSample().Count);

                var name = new MetricName(typeof(CounterTests),
                    "Can_get_all_registered_metrics_as_readonly_and_immutable");

                metrics.Metrics.Counter(typeof(CounterTests),
                    "Can_get_all_registered_metrics_as_readonly_and_immutable");

                Assert.Equal(1, metrics.Metrics.GetSample().Count);

                var value = metrics.Metrics.GetSample()[name];

                Assert.NotNull(value);

                ((CounterMetric) value).Increment();

                Assert.Equal(0, ((CounterMetric) metrics.Metrics.GetSample()[name]).Count);
            }
        }

        [Fact]
        public void Can_get_same_metric_when_metric_exists()
        {
            var counter = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.NotNull(counter);

            var same = _fixture.Metrics.Counter(typeof(CounterTests), "Can_get_same_metric_when_metric_exists");
            Assert.Same(counter, same);
        }
    }
}

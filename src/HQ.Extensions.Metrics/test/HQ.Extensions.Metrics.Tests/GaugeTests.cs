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

using System.Collections.Generic;
using Xunit;

namespace HQ.Extensions.Metrics.Tests.Core
{
    public class GaugeTests : IClassFixture<MetricsFixture>
    {
        public GaugeTests(MetricsFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly MetricsFixture _fixture;

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
            var gauge = _fixture.Metrics.Gauge(typeof(GaugeTests), "Can_use_gauge_metric", () => queue.Count);
            queue.Enqueue(5);
            Assert.Equal(1, gauge.Value);
        }
    }
}

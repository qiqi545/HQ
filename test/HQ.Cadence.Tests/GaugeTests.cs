// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using Xunit;

namespace HQ.Cadence.Tests.Core
{
    public class GaugeTests : IClassFixture<MetricsFixture>
    {
	    readonly MetricsFixture _fixture;

	    public GaugeTests(MetricsFixture fixture)
	    {
		    _fixture = fixture;
	    }

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

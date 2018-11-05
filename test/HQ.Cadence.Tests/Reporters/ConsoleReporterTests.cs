// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using HQ.Cadence.Reporters.Console;
using HQ.Cadence.Tests.Core;
using Microsoft.Extensions.Options;
using Xunit;

namespace HQ.Cadence.Tests.Reporters
{
    public class ConsoleReporterTests : IClassFixture<MetricsFixture>
    {
	    private readonly MetricsFixture _fixture;

	    public ConsoleReporterTests(MetricsFixture fixture)
	    {
		    _fixture = fixture;
	    }

		[Fact]
        public void Can_run_with_known_counters()
        {
            var counter = _fixture.Metrics.Counter(typeof(CounterTests), "Can_run_with_known_counters_counter");
            counter.Increment(100);

            var queue = new Queue<int>();
            _fixture.Metrics.Gauge(typeof(GaugeTests), "Can_run_with_known_counters_gauge", () => queue.Count);
            queue.Enqueue(1);
            queue.Enqueue(2);

	        var registry = new InMemoryMetricsRegistry();
	        var reporter = new ConsoleReporter(registry, Options.Create(new ConsoleReporterOptions()));
	        reporter.Report();
        }
    }
}

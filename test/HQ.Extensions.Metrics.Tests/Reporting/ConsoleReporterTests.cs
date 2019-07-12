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
using HQ.Extensions.Metrics.Reporters.Console;
using Xunit;

namespace HQ.Extensions.Metrics.Tests.Reporting
{
    public class ConsoleReporterTests : IClassFixture<MetricsFixture>
    {
        public ConsoleReporterTests(MetricsFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly MetricsFixture _fixture;

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
            var reporter = new ConsoleReporter(registry,  Microsoft.Extensions.Options.Options.Create(new ConsoleReporterOptions()));
            reporter.Report();
        }
    }
}

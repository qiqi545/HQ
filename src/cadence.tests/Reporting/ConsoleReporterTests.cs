using System;
using System.Collections.Generic;
using cadence.Reporting;
using cadence.tests.Core;
using Xunit;

namespace cadence.tests.Reporting
{
    public class ConsoleReporterTests
    {
        [Fact]
        public void Can_run_with_known_counters()
        {
            var counter = Metrics.Counter(typeof(CounterTests), "Can_run_with_known_counters_counter");
            counter.Increment(100);

            var queue = new Queue<int>();
            Metrics.Gauge(typeof(GaugeTests), "Can_run_with_known_counters_gauge", () => queue.Count);
            queue.Enqueue(1);
            queue.Enqueue(2);

            var reporter = new ConsoleReporter(Console.Out);
            reporter.Run();
        }
    }
}

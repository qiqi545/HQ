using System.Threading.Tasks;
using HQ.Extensions.Metrics.Reporting;
using Microsoft.Extensions.Options;
using Xunit;

namespace HQ.Extensions.Metrics.Tests.Reporting
{
    public class RealtimeReporterTests
    {
        [Fact]
        public async Task Can_instrument_gauge()
        {
            var host = new MetricsHost<RealtimeReporterTests>();
            var options = Options.Create(new MetricsOptions());

            var reporter = new RealtimeReporter(options);
            await reporter.InitializeAsync();

            var changes = 0;
            RealtimeReporter.onGaugeValueChanged = s => changes++;

            host.Gauge("gauge", () => true);
            host.Gauge("gauge", () => true);
            host.Gauge("gauge", () => true);

            Assert.Equal(1, changes);
        }
    }
}

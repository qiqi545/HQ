using System;
using System.Reflection;
using System.Threading.Tasks;
using HQ.Remix;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Metrics.Reporting
{
    public class RealtimeReporter : IMetricsReporter
    {
        private readonly IOptions<MetricsOptions> _options;

        public static Action<string> onGaugeValueChanged;
        
        public RealtimeReporter(IOptions<MetricsOptions> options)
        {
            _options = options;
        }

        public static void NotifyGauge(string value)
        {
            onGaugeValueChanged?.Invoke(value);
        }

        public Task InitializeAsync()
        {
            if(!_options.Value.Filter.HasFlagFast(MetricType.Gauge))
            {
                var changeInValue = typeof(GaugeMetric<>).GetMethod(nameof(GaugeMetric.TryGetChangeInValue));
                var notify = typeof(RealtimeReporter).GetMethod(nameof(NotifyGauge), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                changeInValue.OnExit(notify);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }       
    }
}

using System.Collections.Generic;
using System.Collections.Immutable;
using HQ.Common;
using HQ.Extensions.Caching;

namespace HQ.Extensions.Metrics
{
    public class CacheMetricsStore : IMetricsStore
    {
        private readonly ICache _metrics;

        public IMetric this[MetricName name] => _metrics.Get<IMetric>(name.CacheKey);

        public CacheMetricsStore(ICache metrics)
        {
            _metrics = metrics;
        }

        public IMetric GetOrAdd(MetricName name, IMetric metric)
        {
            var m = _metrics.GetOrAdd(name.CacheKey, metric);
            UpdateManifest(name);
            return m;
        }

        public bool TryGetValue(MetricName name, out IMetric metric)
        {
            metric = _metrics.Get<IMetric>(name.CacheKey);
            return metric != null;
        }

        public bool Contains(MetricName name)
        {
            return TryGetValue(name, out _);
        }

        public void AddOrUpdate<T>(MetricName name, T metric) where T : IMetric
        {
            _metrics.Set(name.CacheKey, metric);
            UpdateManifest(name);
        }

        private static readonly IImmutableDictionary<MetricName, IMetric> NoSample =
            ImmutableDictionary.Create<MetricName, IMetric>();

        public IImmutableDictionary<MetricName, IMetric> GetSample(MetricType filterType = MetricType.None)
        {
            if (filterType.HasFlagFast(MetricType.All))
                return NoSample;
            var filtered = new Dictionary<MetricName, IMetric>();
            foreach (var entry in _metrics.Get<List<MetricName>>(Constants.Categories.Metrics))
            {
                switch (entry.Class.Name)
                {
                    case nameof(GaugeMetric) when filterType.HasFlagFast(MetricType.Gauge):
                    case nameof(CounterMetric) when filterType.HasFlagFast(MetricType.Counter):
                    case nameof(MeterMetric) when filterType.HasFlagFast(MetricType.Meter):
                    case nameof(HistogramMetric) when filterType.HasFlagFast(MetricType.Histogram):
                    case nameof(TimerMetric) when filterType.HasFlagFast(MetricType.Timer):
                        continue;
                    default:
                        filtered.Add(entry, _metrics.Get<IMetric>(entry.CacheKey));
                        break;
                }
            }
            return filtered.ToImmutableDictionary();
        }

        public void Clear()
        {
            if(_metrics is IClearable clearable)
                clearable.Clear();
        }

        private void UpdateManifest(MetricName name)
        {
            var list = _metrics.GetOrAdd(Constants.Categories.Metrics, () => new List<MetricName>());
            if (list.Contains(name))
                return;
            list.Add(name);
            _metrics.Set(Constants.Categories.Metrics, list);
        }
    }
}

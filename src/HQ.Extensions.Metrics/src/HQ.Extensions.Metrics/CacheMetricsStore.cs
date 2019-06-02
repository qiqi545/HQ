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
using System.Collections.Immutable;
using HQ.Extensions.Caching;

namespace HQ.Extensions.Metrics
{
    public class CacheMetricsStore : IMetricsStore
    {
        private static readonly IImmutableDictionary<MetricName, IMetric> NoSample =
            ImmutableDictionary.Create<MetricName, IMetric>();

        private readonly ICache _metrics;

        public CacheMetricsStore(ICache metrics)
        {
            _metrics = metrics;
        }

        public IMetric this[MetricName name] => _metrics.Get<IMetric>(name.CacheKey);

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

        public IImmutableDictionary<MetricName, IMetric> GetSample(MetricType typeFilter = MetricType.None)
        {
            if (typeFilter.HasFlagFast(MetricType.All))
            {
                return NoSample;
            }

            var filtered = new Dictionary<MetricName, IMetric>();
            foreach (var entry in _metrics.Get<List<MetricName>>(Common.Constants.Categories.Metrics))
            {
                switch (entry.Class.Name)
                {
                    case nameof(GaugeMetric) when typeFilter.HasFlagFast(MetricType.Gauge):
                    case nameof(CounterMetric) when typeFilter.HasFlagFast(MetricType.Counter):
                    case nameof(MeterMetric) when typeFilter.HasFlagFast(MetricType.Meter):
                    case nameof(HistogramMetric) when typeFilter.HasFlagFast(MetricType.Histogram):
                    case nameof(TimerMetric) when typeFilter.HasFlagFast(MetricType.Timer):
                        continue;
                    default:
                        filtered.Add(entry, _metrics.Get<IMetric>(entry.CacheKey));
                        break;
                }
            }

            return filtered.ToImmutableDictionary();
        }

        public bool Clear()
        {
            if (!(_metrics is IClearable clear))
            {
                return false;
            }

            clear.Clear();
            return true;
        }

        private void UpdateManifest(MetricName name)
        {
            var list = _metrics.GetOrAdd(Common.Constants.Categories.Metrics, () => new List<MetricName>());
            if (list.Contains(name))
            {
                return;
            }

            list.Add(name);
            _metrics.Set(Common.Constants.Categories.Metrics, list);
        }
    }
}

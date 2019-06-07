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
    public class CacheMetricsStore : CacheKeyValueStore<MetricName, IMetric>
    {
        private readonly ICache _cache;

        private static readonly IImmutableDictionary<MetricName, IMetric> NoSample =
            ImmutableDictionary.Create<MetricName, IMetric>();

        public CacheMetricsStore(ICache cache) : base(cache, Common.Constants.Categories.Metrics)
        {
            _cache = cache;
        }

        public IImmutableDictionary<MetricName, IMetric> GetSample(MetricType typeFilter = MetricType.None)
        {
            if (typeFilter.HasFlagFast(MetricType.All))
            {
                return NoSample;
            }

            var filtered = new Dictionary<MetricName, IMetric>();
            foreach (var entry in _cache.Get<List<MetricName>>(Common.Constants.Categories.Metrics))
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
                        filtered.Add(entry, _cache.Get<IMetric>(entry.CacheKey));
                        break;
                }
            }

            return filtered.ToImmutableDictionary();
        }
    }
}


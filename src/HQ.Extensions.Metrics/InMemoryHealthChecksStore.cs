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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HQ.Extensions.Metrics
{
    public class InMemoryHealthChecksStore : IHealthChecksStore
    {
        private readonly ConcurrentDictionary<MetricName, GaugeMetric<bool>> _metrics;

        public InMemoryHealthChecksStore()
        {
            _metrics = new ConcurrentDictionary<MetricName, GaugeMetric<bool>>();
        }

        public GaugeMetric<bool> GetOrAdd(MetricName name, GaugeMetric<bool> metric)
        {
            return _metrics.GetOrAdd(name, metric);
        }

        public GaugeMetric<bool> this[MetricName name] => _metrics[name];

        public bool TryGetValue(MetricName name, out GaugeMetric<bool> metric)
        {
            return _metrics.TryGetValue(name, out metric);
        }

        public bool Contains(MetricName name)
        {
            return _metrics.ContainsKey(name);
        }

        public GaugeMetric<bool> AddOrUpdate<T>(MetricName name, T metric) where T : GaugeMetric<bool>
        {
            return _metrics.AddOrUpdate(name, metric, (n, m) => m);
        }

        private static readonly IImmutableDictionary<MetricName, GaugeMetric<bool>> NoSample
            = ImmutableDictionary.Create<MetricName, GaugeMetric<bool>>();

        public IImmutableDictionary<MetricName, GaugeMetric<bool>> GetSample(MetricType filterTypes = MetricType.None)
        {
            return filterTypes.HasFlagFast(MetricType.All) ? NoSample : _metrics.ToImmutableDictionary();
        }

        public void Clear()
        {
            _metrics.Clear();
        }
    }
}

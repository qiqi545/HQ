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
using System.Collections.ObjectModel;

namespace HQ.Extensions.Metrics
{
    public class InMemoryMetricsStore : IMetricsStore
    {
        private readonly ConcurrentDictionary<MetricName, IMetric> _metrics;

        public InMemoryMetricsStore()
        {
            _metrics = new ConcurrentDictionary<MetricName, IMetric>();
        }

        public IMetric GetOrAdd(MetricName name, IMetric metric)
        {
            return _metrics.GetOrAdd(name, metric);
        }

        public IMetric this[MetricName name] => _metrics[name];

        public bool TryGetValue(MetricName name, out IMetric metric)
        {
            return _metrics.TryGetValue(name, out metric);
        }

        public bool Contains(MetricName name)
        {
            return _metrics.ContainsKey(name);
        }

        public IMetric AddOrUpdate<T>(MetricName name, T metric) where T : IMetric
        {
            return _metrics.AddOrUpdate(name, metric, (n, m) => m);
        }

        public IReadOnlyDictionary<MetricName, IMetric> AsReadOnly()
        {
            var copy = new Dictionary<MetricName, IMetric>();
            foreach (var entry in _metrics)
                copy.Add(entry.Key, entry.Value.Copy);
            return new ReadOnlyDictionary<MetricName, IMetric>(copy);
        }

        public void Clear()
        {
            _metrics.Clear();
        }
    }
}

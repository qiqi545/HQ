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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HQ.Extensions.Metrics
{
    public class HealthChecksHost : IHealthChecksHost
    {
        private readonly IHealthChecksStore _store;

        public HealthChecksHost(IHealthChecksStore store)
        {
            _store = store;
        }

        public GaugeMetric<bool> HealthCheck<T>(Type type, string name, Func<T, bool> predicate, Func<T> evaluator)
        {
            return GetOrAdd(new MetricName(type, name), new GaugeMetric<bool>(() => predicate(evaluator())));
        }

        public IReadOnlyDictionary<MetricName, IMetric> AsReadOnly()
        {
            return new ReadOnlyDictionary<MetricName, IMetric>(_store.AsReadOnly()
                .ToDictionary(entry => entry.Key, entry => entry.Value.Copy));
        }

        private GaugeMetric<bool> GetOrAdd(MetricName name, GaugeMetric<bool> metric)
        {
            if (_store.Contains(name))
                return _store[name];
            var added = _store.AddOrUpdate(name, metric);
            return added ?? metric;
        }
    }
}

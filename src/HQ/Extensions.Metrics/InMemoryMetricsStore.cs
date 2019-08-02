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
using System.Collections.Immutable;
using HQ.Data.Contracts.Runtime;

namespace HQ.Extensions.Metrics
{
    public class InMemoryMetricsStore : InMemoryKeyValueStore<MetricName, IMetric>, IMetricsStore
    {
        private static readonly IImmutableDictionary<MetricName, IMetric> NoSample =
            ImmutableDictionary.Create<MetricName, IMetric>();

        public IImmutableDictionary<MetricName, IMetric> GetSample(MetricType typeFilter = MetricType.All)
        {
            if (typeFilter.HasFlagFast(MetricType.All))
            {
                return NoSample;
            }

            var filtered = new Dictionary<MetricName, IMetric>();
            foreach (var entry in Memory)
            {
                switch (entry.Value)
                {
                    case GaugeMetric _ when typeFilter.HasFlagFast(MetricType.Gauge):
                    case CounterMetric _ when typeFilter.HasFlagFast(MetricType.Counter):
                    case MeterMetric _ when typeFilter.HasFlagFast(MetricType.Meter):
                    case HistogramMetric _ when typeFilter.HasFlagFast(MetricType.Histogram):
                    case TimerMetric _ when typeFilter.HasFlagFast(MetricType.Timer):
                        continue;
                    default:
                        filtered.Add(entry.Key, entry.Value);
                        break;
                }
            }

            return filtered.ToImmutableSortedDictionary(k => k.Key, v =>
            {
	            switch (v.Value)
	            {
					case GaugeMetric gauge:
			            return gauge.Copy();
		            case CounterMetric counter:
			            return counter.Copy();
		            case MeterMetric meter:
			            return meter.Copy();
		            case HistogramMetric histogram:
			            return histogram.Copy();
		            case TimerMetric timer:
			            return timer.Copy();
					default:
						throw new ArgumentException();
	            }
            });
        }
    }
}

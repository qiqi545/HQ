// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HQ.Cadence
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

		public IReadOnlyDictionary<MetricName, GaugeMetric<bool>> AsReadOnly()
		{
			return new ReadOnlyDictionary<MetricName, GaugeMetric<bool>>(_metrics);
		}

		public void Clear()
		{
			_metrics.Clear();
		}
	}
}
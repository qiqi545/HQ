// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HQ.Cadence
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
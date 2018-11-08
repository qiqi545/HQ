// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Cadence
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

		private GaugeMetric<bool> GetOrAdd(MetricName name, GaugeMetric<bool> metric)
		{
			if (_store.Contains(name))
				return _store[name];
			var added = _store.AddOrUpdate(name, metric);
			return added ?? metric;
		}
	}
}
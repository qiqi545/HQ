// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Cadence
{
	public interface IMetricsStore
	{
		IMetric GetOrAdd(MetricName name, IMetric metric);
		IMetric this[MetricName name] { get; }
		bool TryGetValue(MetricName name, out IMetric metric);
		bool Contains(MetricName name);
		IMetric AddOrUpdate<T>(MetricName name, T metric, Func<MetricName, IMetric, IMetric> func) where T : IMetric;
		IReadOnlyDictionary<MetricName, IMetric> AsReadOnly();
		void Clear();
	}
}
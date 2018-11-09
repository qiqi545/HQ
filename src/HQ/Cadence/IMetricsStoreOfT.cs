// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;

namespace HQ.Cadence
{
	public interface IMetricsStore<TFilter> where TFilter : IMetric
	{
		TFilter this[MetricName name] { get; }
		TFilter GetOrAdd(MetricName name, TFilter metric);
		bool TryGetValue(MetricName name, out TFilter metric);
		bool Contains(MetricName name);
		TFilter AddOrUpdate<T>(MetricName name, T metric) where T : TFilter;
		IReadOnlyDictionary<MetricName, TFilter> AsReadOnly();
		void Clear();
	}
}
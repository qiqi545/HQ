// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Cadence
{
	public interface IMetricsHost
	{
		IReadOnlyDictionary<MetricName, IMetric> All { get; }
		GaugeMetric<T> Gauge<T>(Type type, string name, Func<T> evaluator);
		CounterMetric Counter(Type type, string name);
		HistogramMetric Histogram(Type type, string name, SampleType sampleType);
		MeterMetric Meter(Type type, string name, string eventType, TimeUnit rateUnit);
		TimerMetric Timer(Type type, string name, TimeUnit durationUnit, TimeUnit rateUnit);
		void Clear();
	}
}
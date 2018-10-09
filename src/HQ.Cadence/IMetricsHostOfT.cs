// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Cadence
{
	public interface IMetricsHost<out TOwner>
	{
		IReadOnlyDictionary<MetricName, IMetric> All { get; }
		GaugeMetric<T> Gauge<T>(string name, Func<T> evaluator);
		CounterMetric Counter(string name);
		HistogramMetric Histogram(string name, SampleType sampleType);
		MeterMetric Meter(string name, string eventType, TimeUnit rateUnit);
		TimerMetric Timer(string name, TimeUnit durationUnit, TimeUnit rateUnit);
		void Clear();
	}
}
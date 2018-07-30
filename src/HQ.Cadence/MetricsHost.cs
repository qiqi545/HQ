// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Cadence
{
	public class MetricsHost : IMetricsHost
	{
		readonly IMetricsStore _store;
		
		public MetricsHost(IMetricsStore store)
		{
			_store = store;
		}

		/// <summary>
		/// Creates a new gauge metric and registers it under the given type and name
		/// </summary>
		/// <typeparam name="T">The type the gauge measures</typeparam>
		/// <param name="type">The type that owns the metric</param>
		/// <param name="name">The metric name</param>
		/// <param name="evaluator">The gauge evaluation function</param>
		/// <returns></returns>
		public GaugeMetric<T> Gauge<T>(Type type, string name, Func<T> evaluator)
		{
			return GetOrAdd(new MetricName(type, name), new GaugeMetric<T>(evaluator));
		}

		/// <summary>
		/// Creates a new counter metric and registers it under the given type and name
		/// </summary>
		/// <param name="type">The type that owns the metric</param>
		/// <param name="name">The metric name</param>
		/// <returns></returns>
		public CounterMetric Counter(Type type, string name)
		{
			return GetOrAdd(new MetricName(type, name), new CounterMetric());
		}

		/// <summary>
		/// Creates a new histogram metric and registers it under the given type and name
		/// </summary>
		/// <param name="type">The type that owns the metric</param>
		/// <param name="name">The metric name</param>
		/// <param name="sampleType">The sample type</param>
		/// <returns></returns>
		public HistogramMetric Histogram(Type type, string name, SampleType sampleType)
		{
			return GetOrAdd(new MetricName(type, name), new HistogramMetric(sampleType));
		}

		/// <summary>
		/// Creates a new meter metric and registers it under the given type and name
		/// </summary>
		/// <param name="type">The type that owns the metric</param>
		/// <param name="name">The metric name</param>
		/// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
		/// <param name="unit">The rate unit of the new meter</param>
		/// <returns></returns>
		public MeterMetric Meter(Type type, string name, string eventType, TimeUnit rateUnit)
		{
			var metricName = new MetricName(type, name);
			if (_store.TryGetValue(metricName, out var existingMetric))
			{
				return (MeterMetric)existingMetric;
			}

			var metric = MeterMetric.New(eventType, rateUnit);
			var justAddedMetric = _store.GetOrAdd(metricName, metric);
			return justAddedMetric == null ? metric : (MeterMetric)justAddedMetric;
		}

		/// <summary>
		/// reates a new timer metric and registers it under the given type and name
		/// </summary>
		/// <param name="type">The type that owns the metric</param>
		/// <param name="name">The metric name</param>
		/// <param name="durationUnit">The duration scale unit of the new timer</param>
		/// <param name="rateUnit">The rate unit of the new timer</param>
		/// <returns></returns>
		public TimerMetric Timer(Type type, string name, TimeUnit durationUnit, TimeUnit rateUnit)
		{
			var metricName = new MetricName(type, name);
			if (_store.TryGetValue(metricName, out var existingMetric))
			{
				return (TimerMetric)existingMetric;
			}

			var metric = new TimerMetric(durationUnit, rateUnit);
			var justAddedMetric = _store.GetOrAdd(metricName, metric);
			return justAddedMetric == null ? metric : (TimerMetric)justAddedMetric;
		}

	    /// <summary>
		/// Returns a copy of all currently registered metrics in an immutable collection
		/// </summary>
		public IReadOnlyDictionary<MetricName, IMetric> All => _store.AsReadOnly();

		/// <summary>
		/// Clears all previously registered metrics
		/// </summary>
		public void Clear()
		{
			_store.Clear();
		}

		T GetOrAdd<T>(MetricName name, T metric) where T : IMetric
		{
			if (_store.Contains(name))
			{
				return (T)_store[name];
			}

			IMetric added = _store.AddOrUpdate(name, metric, (n, m) => m);

			return added == null ? metric : (T)added;
		}
	}
}
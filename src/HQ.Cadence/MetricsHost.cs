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

namespace HQ.Cadence
{
    public class MetricsHost : IMetricsHost
    {
        private readonly IMetricsStore _store;

        public MetricsHost(IMetricsStore store)
        {
            _store = store;
        }

        /// <summary>
        ///     Creates a new gauge metric and registers it under the given type and name
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
        ///     Creates a new counter metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public CounterMetric Counter(Type type, string name)
        {
            return GetOrAdd(new MetricName(type, name), new CounterMetric());
        }

        /// <summary>
        ///     Creates a new histogram metric and registers it under the given type and name
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
        ///     Creates a new meter metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
        /// <param name="rateUnit">The rate unit of the new meter</param>
        /// <returns></returns>
        public MeterMetric Meter(Type type, string name, string eventType, TimeUnit rateUnit)
        {
            var metricName = new MetricName(type, name);
            if (_store.TryGetValue(metricName, out var existingMetric)) return (MeterMetric) existingMetric;

            var metric = MeterMetric.New(eventType, rateUnit);
            var justAddedMetric = _store.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (MeterMetric) justAddedMetric;
        }

        /// <summary>
        ///     reates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="type">The type that owns the metric</param>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public TimerMetric Timer(Type type, string name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
            var metricName = new MetricName(type, name);
            if (_store.TryGetValue(metricName, out var existingMetric)) return (TimerMetric) existingMetric;

            var metric = new TimerMetric(durationUnit, rateUnit);
            var justAddedMetric = _store.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (TimerMetric) justAddedMetric;
        }

        /// <summary>
        ///     Returns a copy of all currently registered metrics in an immutable collection
        /// </summary>
        public IReadOnlyDictionary<MetricName, IMetric> AsReadOnly()
        {
            return _store.AsReadOnly();
        }

        /// <summary>
        ///     Clears all previously registered metrics
        /// </summary>
        public void Clear()
        {
            _store.Clear();
        }

        private T GetOrAdd<T>(MetricName name, T metric) where T : IMetric
        {
            if (_store.Contains(name)) return (T) _store[name];

            var added = _store.AddOrUpdate(name, metric);

            return added == null ? metric : (T) added;
        }
    }
}

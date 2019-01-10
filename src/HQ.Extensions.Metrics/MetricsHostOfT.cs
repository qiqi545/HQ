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

namespace HQ.Extensions.Metrics
{
    public class MetricsHost<TOwner> : IMetricsHost<TOwner>
    {
        private readonly IMetricsHost _host;

        public MetricsHost(IMetricsHost host)
        {
            _host = host;
        }

        /// <summary>
        ///     Creates a new gauge metric and registers it under the given type and name
        /// </summary>
        /// <typeparam name="T">The type the gauge measures</typeparam>
        /// <param name="name">The metric name</param>
        /// <param name="evaluator">The gauge evaluation function</param>
        /// <returns></returns>
        public GaugeMetric<T> Gauge<T>(string name, Func<T> evaluator)
        {
            return _host.Gauge(typeof(TOwner), name, evaluator);
        }

        /// <summary>
        ///     Creates a new counter metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public CounterMetric Counter(string name)
        {
            return _host.Counter(typeof(TOwner), name);
        }

        /// <summary>
        ///     Creates a new histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="sampleType">The sample type</param>
        /// <returns></returns>
        public HistogramMetric Histogram(string name, SampleType sampleType)
        {
            return _host.Histogram(typeof(TOwner), name, sampleType);
        }

        /// <summary>
        ///     Creates a new meter metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
        /// <param name="rateUnit">The rate unit of the new meter</param>
        /// <returns></returns>
        public MeterMetric Meter(string name, string eventType, TimeUnit rateUnit)
        {
            return _host.Meter(typeof(TOwner), name, eventType, rateUnit);
        }

        /// <summary>
        ///     reates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public TimerMetric Timer(string name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
            return _host.Timer(typeof(TOwner), name, durationUnit, rateUnit);
        }

        /// <summary>
        ///     Returns a copy of all currently registered metrics in an immutable collection
        /// </summary>
        public IReadOnlyDictionary<MetricName, IMetric> All => _host.AsReadOnly();

        /// <summary>
        ///     Clears all previously registered metrics
        /// </summary>
        public void Clear()
        {
            _host.Clear();
        }
    }
}

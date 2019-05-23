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
using HQ.Common;

namespace HQ.Extensions.Metrics
{
    public abstract class GaugeMetric : IMetric
    {
        public abstract string ValueAsString { get; }
        public abstract bool IsNumeric { get; }
        public abstract bool IsBoolean { get; }
        public abstract MetricName Name { get; }
    }

    /// <summary>
    ///     A gauge metric is an instantaneous reading of a particular value. To
    ///     instrument a queue's depth, for example:
    ///     <example>
    ///         <code> 
    /// var queue = new Queue{int}();
    /// var gauge = new GaugeMetric{int}(() => queue.Count);
    /// </code>
    ///     </example>
    /// </summary>
    public class GaugeMetric<T> : GaugeMetric
    {
        private readonly Func<T> _evaluator;

        public override MetricName Name { get; }

        internal GaugeMetric(MetricName metricName, Func<T> evaluator)
        {
            Name = metricName;
            _evaluator = evaluator;
            IsNumeric = typeof(T).IsNumeric();
            IsBoolean = typeof(T).IsTruthy();
        }

        public T Value => _evaluator();

        public override string ValueAsString => Value?.ToString();

        public override bool IsNumeric { get; }

        public override bool IsBoolean { get; }
    }
}

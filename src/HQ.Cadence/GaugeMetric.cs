// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Newtonsoft.Json;

namespace HQ.Cadence
{
    public abstract class GaugeMetric
    {
        public abstract string ValueAsString { get; }
    }

    /// <summary>
    /// A gauge metric is an instantaneous reading of a particular value. To
    /// instrument a queue's depth, for example:
    /// <example>
    /// <code> 
    /// var queue = new Queue{int}();
    /// var gauge = new GaugeMetric{int}(() => queue.Count);
    /// </code>
    /// </example>
    /// </summary>
    public sealed class GaugeMetric<T> : GaugeMetric, IMetric
    {
        readonly Func<T> _evaluator;

        public GaugeMetric(Func<T> evaluator)
        {
            _evaluator = evaluator;
        }

        public T Value => _evaluator.Invoke();

	    public override string ValueAsString => Value.ToString();

	    [JsonIgnore]
        public IMetric Copy => new GaugeMetric<T>(_evaluator);
    }
}
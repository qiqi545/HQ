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
using System.Runtime.Serialization;
using HQ.Extensions.Metrics.Internal;

namespace HQ.Extensions.Metrics
{
    /// <summary>
    ///     An atomic counter metric
    /// </summary>
    public sealed class CounterMetric : IMetric, IComparable<CounterMetric>, IComparable
    {
        private readonly AtomicLong _count = new AtomicLong(0);

        internal CounterMetric(MetricName metricName) { Name = metricName; }


        private CounterMetric(MetricName metricName, AtomicLong count)
        {
	        Name = metricName;
			_count = count;
        }
        internal IMetric Copy()
        {
	        var copy = new CounterMetric(Name, new AtomicLong(_count));
	        return copy;
        }

		public long Count => _count.Get();

        [IgnoreDataMember] public MetricName Name { get; }

        public void Increment()
        {
            Increment(1);
        }

        public long Increment(long amount)
        {
            return _count.AddAndGet(amount);
        }

        public long Decrement()
        {
            return Decrement(1);
        }

        public long Decrement(long amount)
        {
            return _count.AddAndGet(0 - amount);
        }

        public void Clear()
        {
            _count.Set(0);
        }

        public int CompareTo(CounterMetric other)
        {
	        if (ReferenceEquals(this, other)) return 0;
	        if (ReferenceEquals(null, other)) return 1;
	        return Name.CompareTo(other.Name);
        }

        public int CompareTo(object obj)
        {
	        if (ReferenceEquals(null, obj)) return 1;
	        if (ReferenceEquals(this, obj)) return 0;
	        return obj is CounterMetric other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(CounterMetric)}");
        }

        public static bool operator <(CounterMetric left, CounterMetric right)
        {
	        return Comparer<CounterMetric>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(CounterMetric left, CounterMetric right)
        {
	        return Comparer<CounterMetric>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(CounterMetric left, CounterMetric right)
        {
	        return Comparer<CounterMetric>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(CounterMetric left, CounterMetric right)
        {
	        return Comparer<CounterMetric>.Default.Compare(left, right) >= 0;
        }

        public int CompareTo(IMetric other)
        {
	        return other.Name.CompareTo(Name);
        }
    }
}

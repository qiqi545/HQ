// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Cadence.Internal;
using Newtonsoft.Json;

namespace HQ.Cadence
{
	/// <summary>
	///     An atomic counter metric
	/// </summary>
	public sealed class CounterMetric : IMetric
	{
		private readonly AtomicLong _count = new AtomicLong(0);

		public CounterMetric()
		{
		}

		private CounterMetric(long count)
		{
			_count.Set(count);
		}

		public long Count => _count.Get();

		[JsonIgnore] public IMetric Copy => new CounterMetric(_count.Get());

		public void Increment()
		{
			Increment(1);
		}

		public void Increment(long amount)
		{
			_count.AddAndGet(amount);
		}

		public void Decrement()
		{
			Decrement(1);
		}

		public void Decrement(long amount)
		{
			_count.AddAndGet(0 - amount);
		}

		public void Clear()
		{
			_count.Set(0);
		}
	}
}
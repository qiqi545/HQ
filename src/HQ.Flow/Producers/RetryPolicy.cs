// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Flow.Producers
{
	public class RetryPolicy
	{
		private readonly IDictionary<int, RetryDecision> _rules;
		private RetryDecision _default = RetryDecision.Requeue;

		public RetryPolicy()
		{
			_rules = new Dictionary<int, RetryDecision>();
			RequeueInterval = a => TimeSpan.FromSeconds(5 + Math.Pow(a, 4));
		}

		public Func<int, TimeSpan> RequeueInterval { get; set; }

		public void Default(RetryDecision action)
		{
			_default = action;
		}

		public void After(int tries, RetryDecision action)
		{
			_rules.Add(tries, action);
		}

		public void Clear()
		{
			_rules.Clear();
		}

		public RetryDecision DecideOn<T>(T @event, int attempts)
		{
			foreach (var threshold in _rules.Keys.OrderBy(k => k).Where(threshold => attempts >= threshold))
				return _rules[threshold];
			return _default;
		}
	}
}
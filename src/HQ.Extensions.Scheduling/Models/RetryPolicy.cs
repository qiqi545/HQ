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
using System.Linq;

namespace HQ.Extensions.Scheduling.Models
{
	/// <summary>
	///     A retry policy, for use with <see cref="PushQueue{T}" />
	/// </summary>
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
			{
				return _rules[threshold];
			}

			return _default;
		}
	}
}
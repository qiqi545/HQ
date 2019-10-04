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
using System.Diagnostics;

namespace HQ.Extensions.Metrics
{
	/// <summary>
	///     Wraps a timing closure with additional data.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TimerHandle<T>
	{
		private readonly Stopwatch _stopwatch;

		public TimerHandle(Stopwatch stopwatch)
		{
			_stopwatch = stopwatch;
			StartedAt = default;
			StoppedAt = default;
			Value = default;
		}

		public DateTimeOffset? StartedAt { get; private set; }
		public DateTimeOffset? StoppedAt { get; private set; }
		public TimeSpan Elapsed => _stopwatch.Elapsed;

		public bool IsStarted => StartedAt.HasValue;
		public bool IsStopped => StoppedAt.HasValue;
		public bool IsRunning => IsStarted && !IsStopped;

		public T Value { get; set; }

		public void Start()
		{
			_stopwatch.Start();
			StartedAt = DateTimeOffset.UtcNow;
		}

		public void Stop()
		{
			_stopwatch.Stop();
			StoppedAt = DateTimeOffset.UtcNow;
		}

		public static implicit operator T(TimerHandle<T> h)
		{
			return h.Value;
		}
	}
}
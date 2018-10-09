// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading;
using System.Threading.Tasks;
using HQ.Cadence.Internal;
using HQ.Cadence.Stats;
using Newtonsoft.Json;

namespace HQ.Cadence
{
	/// <summary>
	///     A meter metric which measures mean throughput and one-, five-, and fifteen-minute exponentially-weighted moving
	///     average throughputs
	/// </summary>
	/// <see href="http://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average">EMA</see>
	public class MeterMetric : IMetric, IMetered
	{
		private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

		private readonly AtomicLong _count = new AtomicLong();
		private readonly EWMA _m15Rate = EWMA.FifteenMinuteEWMA();

		private readonly EWMA _m1Rate = EWMA.OneMinuteEWMA();
		private readonly EWMA _m5Rate = EWMA.FiveMinuteEWMA();
		private readonly long _startTime = DateTime.Now.Ticks;

		private CancellationTokenSource _cancel;
		private Task _task;

		private MeterMetric(string eventType, TimeUnit rateUnit)
		{
			EventType = eventType;
			RateUnit = rateUnit;
		}

		/// <summary>
		///     Returns the meter's rate unit
		/// </summary>
		/// <returns></returns>
		public TimeUnit RateUnit { get; }

		/// <summary>
		///     Returns the type of events the meter is measuring
		/// </summary>
		/// <returns></returns>
		public string EventType { get; }

		/// <summary>
		///     Returns the number of events which have been marked
		/// </summary>
		/// <returns></returns>
		public long Count => _count.Get();

		/// <summary>
		///     Returns the fifteen-minute exponentially-weighted moving average rate at
		///     which events have occured since the meter was created
		///     <remarks>
		///         This rate has the same exponential decay factor as the fifteen-minute load
		///         average in the top Unix command.
		///     </remarks>
		/// </summary>
		public double FifteenMinuteRate => _m15Rate.Rate(RateUnit);

		/// <summary>
		///     Returns the five-minute exponentially-weighted moving average rate at
		///     which events have occured since the meter was created
		///     <remarks>
		///         This rate has the same exponential decay factor as the five-minute load
		///         average in the top Unix command.
		///     </remarks>
		/// </summary>
		public double FiveMinuteRate => _m5Rate.Rate(RateUnit);

		/// <summary>
		///     Returns the mean rate at which events have occured since the meter was created
		/// </summary>
		public double MeanRate
		{
			get
			{
				if (Count != 0)
				{
					var elapsed = DateTime.Now.Ticks - _startTime;
					return ConvertNanosRate(Count / (double) elapsed);
				}

				return 0.0;
			}
		}

		/// <summary>
		///     Returns the one-minute exponentially-weighted moving average rate at
		///     which events have occured since the meter was created
		///     <remarks>
		///         This rate has the same exponential decay factor as the one-minute load
		///         average in the top Unix command.
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		public double OneMinuteRate => _m1Rate.Rate(RateUnit);

		[JsonIgnore] public IMetric Copy => new MeterMetric(EventType, RateUnit);

		public static MeterMetric New(string eventType, TimeUnit rateUnit)
		{
			var meter = new MeterMetric(eventType, rateUnit);
			meter.Start();
			return meter;
		}

		public Task Start()
		{
			_cancel = new CancellationTokenSource();
			_task = OnTimer(Tick, _cancel.Token);
			return _task;
		}

		public void Stop()
		{
			if (_task.IsCompleted)
			{
				_task?.Dispose();
			}
			else
			{
				_cancel.Cancel();
				_task?.Dispose();
				_cancel.Dispose();
			}
		}

		public async Task OnTimer(Action action, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(Interval, cancellationToken);

				if (!cancellationToken.IsCancellationRequested)
					action();
			}
		}

		private void Tick()
		{
			if (_cancel.IsCancellationRequested)
				return;
			_m1Rate.Tick();
			_m5Rate.Tick();
			_m15Rate.Tick();
		}

		/// <summary>
		///     Mark the occurrence of an event
		/// </summary>
		public void Mark()
		{
			Mark(1);
		}

		/// <summary>
		///     Mark the occurrence of a given number of events
		/// </summary>
		public void Mark(long n)
		{
			_count.AddAndGet(n);
			_m1Rate.Update(n);
			_m5Rate.Update(n);
			_m15Rate.Update(n);
		}

		private double ConvertNanosRate(double ratePerNs)
		{
			return ratePerNs * RateUnit.ToNanos(1);
		}
	}
}
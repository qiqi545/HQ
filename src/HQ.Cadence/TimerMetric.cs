// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace HQ.Cadence
{
    /// <summary>
    /// A timer metric which aggregates timing durations and provides duration statistics, plus throughput statistics via <see cref="MeterMetric" />.
    /// </summary>
    public class TimerMetric : IMetric, IMetered
    {
        readonly MeterMetric _meter;
        readonly HistogramMetric _histogram;

	    /// <summary>
	    ///  Returns the timer's duration scale unit
	    /// </summary>
	    public TimeUnit DurationUnit { get; }

	    /// <summary>
	    /// Returns the meter's rate unit
	    /// </summary>
	    /// <returns></returns>
	    public TimeUnit RateUnit { get; }

		public TimerMetric(TimeUnit durationUnit, TimeUnit rateUnit) : this(durationUnit, rateUnit, MeterMetric.New("calls", rateUnit), new HistogramMetric(SampleType.Biased), clear: true)
        {

        }

        TimerMetric(TimeUnit durationUnit, TimeUnit rateUnit, MeterMetric meter, HistogramMetric histogram, bool clear)
        {
            DurationUnit = durationUnit;
            RateUnit = rateUnit;
            _meter = meter;
            _histogram = histogram;
            if(clear)
            {
                Clear();
            }
        }
		
        /// <summary>
        /// Clears all recorded durations
        /// </summary>
        public void Clear()
        {
            _histogram.Clear();
        }

        public void Update(long duration, TimeUnit durationUnit)
        {
            Update(durationUnit.ToNanos(duration));
        }

	    private void Update(long duration)
	    {
		    if (duration < 0) return;
		    _histogram.Update(duration);
		    _meter.Mark();
	    }

		/// <summary>
		/// Times and records the duration of an event
		/// </summary>
		/// <typeparam name="T">The type of the value returned by the event</typeparam>
		/// <param name="event">A function whose duration should be timed</param>
		public T Time<T>(Func<T> @event)
        {
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                return @event.Invoke();
            }
            finally
            {
                stopwatch.Stop();
                Update(stopwatch.Elapsed.Ticks);
            }
        }

        /// <summary>
        ///  Returns the number of events which have been marked
        /// </summary>
        /// <returns></returns>
        public long Count
        {
            get { return _histogram.Count; }
        }

        /// <summary>
        /// Returns the fifteen-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the fifteen-minute load
        /// average in the top Unix command.
        /// </remarks> 
        /// </summary>
        public double FifteenMinuteRate
        {
            get { return _meter.FifteenMinuteRate; }
        }

        /// <summary>
        /// Returns the five-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the five-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        public double FiveMinuteRate
        {
            get { return _meter.FiveMinuteRate; }
        }

        /// <summary>
        /// Returns the mean rate at which events have occured since the meter was created
        /// </summary>
        public double MeanRate
        {
            get { return _meter.MeanRate; }
        }

        /// <summary>
        /// Returns the one-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the one-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public double OneMinuteRate => _meter.OneMinuteRate;

	    /// <summary>
        /// Returns the longest recorded duration
        /// </summary>
        public double Max => ConvertFromNanos(_histogram.Max);

	    /// <summary>
        /// Returns the shortest recorded duration
        /// </summary>
        public double Min => ConvertFromNanos(_histogram.Min);

	    /// <summary>
        ///  Returns the arithmetic mean of all recorded durations
        /// </summary>
        public double Mean => ConvertFromNanos(_histogram.Mean);

	    /// <summary>
        /// Returns the standard deviation of all recorded durations
        /// </summary>
        public double StdDev => ConvertFromNanos(_histogram.StdDev);

	    /// <summary>
        /// Returns an array of durations at the given percentiles
        /// </summary>
        public double[] Percentiles(params double[] percentiles)
        {
            var scores = _histogram.Percentiles(percentiles);
            for (var i = 0; i < scores.Length; i++)
            {
                scores[i] = ConvertFromNanos(scores[i]);
            }

            return scores;
        }

        /// <summary>
        /// Returns the type of events the meter is measuring
        /// </summary>
        /// <returns></returns>
        public string EventType => _meter.EventType;

	    /// <summary>
        /// Returns a list of all recorded durations in the timers's sample
        /// </summary>
        public ICollection<double> Values
        {
            get
            {
                return _histogram.Values.Select(value => ConvertFromNanos(value)).ToList();
            }
        }
		
        private double ConvertFromNanos(double nanos)
        {
            return nanos / TimeUnit.Nanoseconds.Convert(1, DurationUnit);
        }
        
        [JsonIgnore]
        public IMetric Copy
        {
            get
            {
                var copy = new TimerMetric(
                    DurationUnit, RateUnit, _meter, _histogram, false /* clear */
                    );
                return copy;
            }
        }
    }
}
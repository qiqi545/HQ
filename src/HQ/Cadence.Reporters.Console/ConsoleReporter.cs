// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.IO;
using System.Threading;
using HQ.Cadence.Reporters.Console.Internal;
using Microsoft.Extensions.Options;

namespace HQ.Cadence.Reporters.Console
{
	/// <summary>
	///     A simple reporter which prints out application metrics to a <see cref="TextWriter" /> periodically
	/// </summary>
	public sealed class ConsoleReporter : PeriodicReporter
	{
		private readonly TextWriter _out;
		private readonly IMetricsRegistry _registry;
		private readonly IOptions<ConsoleReporterOptions> _options;

		public ConsoleReporter(IMetricsRegistry registry, IOptions<ConsoleReporterOptions> options) : this(System.Console.Out, registry, options) { }

		private ConsoleReporter(TextWriter @out, IMetricsRegistry registry, IOptions<ConsoleReporterOptions> options) : base(options.Value.Interval)
		{
			_out = @out;
			_registry = registry;
			_options = options;
		}

		public override void Report(CancellationToken? cancellationToken = null)
		{
			if (!TryWrite(_registry, _out, cancellationToken) && _options.Value.StopOnError)
				Stop();
		}

		internal static bool TryWrite(IMetricsRegistry registry, TextWriter @out, CancellationToken? cancellationToken)
		{
			if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
				return true;

			try
			{
				cancellationToken?.ThrowIfCancellationRequested();

				var now = DateTime.Now;
				var dateTime = $"{now.ToShortDateString()} {now.ToShortTimeString()}";
				@out.Write(dateTime);
				@out.Write(' ');
				for (var i = 0; i < 80 - dateTime.Length - 1; i++) @out.Write('=');
				@out.WriteLine();

				foreach (var host in registry)
				foreach (var entry in host.AsReadOnly.Sort())
				{
					@out.Write(entry.Key);
					@out.WriteLine(':');

					foreach (var subEntry in entry.Value)
					{
						@out.Write("  ");
						@out.Write(subEntry.Key);
						@out.WriteLine(':');

						var metric = subEntry.Value;
						switch (metric)
						{
							case GaugeMetric _:
								WriteGauge(@out, (GaugeMetric) metric);
								break;
							case CounterMetric _:
								WriteCounter(@out, (CounterMetric) metric);
								break;
							case HistogramMetric _:
								WriteHistogram(@out, (HistogramMetric) metric);
								break;
							case MeterMetric _:
								WriteMetered(@out, (MeterMetric) metric);
								break;
							case TimerMetric _:
								WriteTimer(@out, (TimerMetric) metric);
								break;
						}

						@out.WriteLine();
					}

					@out.WriteLine();
					@out.Flush();
				}

				return true;
			}
			catch (Exception e)
			{
				@out.WriteLine(e.StackTrace);
				return false;
			}
		}

		private static void WriteGauge(TextWriter writer, GaugeMetric gauge)
		{
			writer.Write("    value = ");
			writer.WriteLine(gauge.ValueAsString);
		}

		private static void WriteCounter(TextWriter writer, CounterMetric counter)
		{
			writer.Write("    count = ");
			writer.WriteLine(counter.Count);
		}

		private static void WriteMetered(TextWriter writer, IMetered meter)
		{
			var unit = Abbreviate(meter.RateUnit);
			writer.Write("             count = {0}\n", meter.Count);
			writer.Write("         mean rate = {0} {1}/{2}\n", meter.MeanRate, meter.EventType, unit);
			writer.Write("     1-minute rate = {0} {1}/{2}\n", meter.OneMinuteRate, meter.EventType, unit);
			writer.Write("     5-minute rate = {0} {1}/{2}\n", meter.FiveMinuteRate, meter.EventType, unit);
			writer.Write("    15-minute rate = {0} {1}/{2}\n", meter.FifteenMinuteRate, meter.EventType, unit);
		}

		private static void WriteHistogram(TextWriter writer, HistogramMetric histogram)
		{
			var percentiles = histogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

			writer.Write("               min = %{0:2}\n", histogram.Min);
			writer.Write("               max = %{0:2}\n", histogram.Max);
			writer.Write("              mean = %{0:2}\n", histogram.Mean);
			writer.Write("            stddev = %{0:2}\n", histogram.StdDev);
			writer.Write("            median = %{0:2}\n", percentiles[0]);
			writer.Write("              75%% <= %{0:2}\n", percentiles[1]);
			writer.Write("              95%% <= %{0:2}\n", percentiles[2]);
			writer.Write("              98%% <= %{0:2}\n", percentiles[3]);
			writer.Write("              99%% <= %{0:2}\n", percentiles[4]);
			writer.Write("            99.9%% <= %{0:2}\n", percentiles[5]);
		}

		private static void WriteTimer(TextWriter writer, TimerMetric timer)
		{
			WriteMetered(writer, timer);

			var durationUnit = Abbreviate(timer.DurationUnit);

			var percentiles = timer.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

			writer.Write("               min = %{0:2}{1}\n", timer.Min, durationUnit);
			writer.Write("               max = %{0:2}{1}\n", timer.Max, durationUnit);
			writer.Write("              mean = %{0:2}{1}\n", timer.Mean, durationUnit);
			writer.Write("            stddev = %{0:2}{1}\n", timer.StdDev, durationUnit);
			writer.Write("            median = %{0:2}{1}\n", percentiles[0], durationUnit);
			writer.Write("              75%% <= %{0:2}{1}\n", percentiles[1], durationUnit);
			writer.Write("              95%% <= %{0:2}{1}\n", percentiles[2], durationUnit);
			writer.Write("              98%% <= %{0:2}{1}\n", percentiles[3], durationUnit);
			writer.Write("              99%% <= %{0:2}{1}\n", percentiles[4], durationUnit);
			writer.Write("            99.9%% <= %{0:2}{1}\n", percentiles[5], durationUnit);
		}

		private static string Abbreviate(TimeUnit unit)
		{
			switch (unit)
			{
				case TimeUnit.Nanoseconds:
					return "ns";
				case TimeUnit.Microseconds:
					return "us";
				case TimeUnit.Milliseconds:
					return "ms";
				case TimeUnit.Seconds:
					return "s";
				case TimeUnit.Minutes:
					return "m";
				case TimeUnit.Hours:
					return "h";
				case TimeUnit.Days:
					return "d";
				default:
					throw new ArgumentOutOfRangeException(nameof(unit));
			}
		}
	}
}
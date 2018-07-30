// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HQ.Cadence.Reporters.Console.Internal;
using Microsoft.Extensions.Options;

namespace HQ.Cadence.Reporters.Console
{
	/// <summary>
	///  A simple reporter which prints out application metrics to a <see cref="TextWriter" /> periodically
	/// </summary>
	public sealed class ConsoleReporter : IMetricsReporter
	{
		readonly IMetricsRegistry _registry;
		readonly TextWriter _out;
		readonly TimeSpan _interval;
		CancellationTokenSource _cancel;

		Task _task;

		internal ConsoleReporter(TextWriter @out)
		{
			_out = @out;
		}

		public ConsoleReporter(IMetricsRegistry registry, IOptions<ConsoleReporterOptions> options)
		{
			_registry = registry;
			_out = System.Console.Out;
			_interval = options?.Value?.Interval ?? TimeSpan.FromSeconds(5);
		}

		public Task Start()
		{
			_cancel = new CancellationTokenSource();
			_task = OnTimer(()=> Report(_cancel.Token), _cancel.Token);
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

		public void Report(CancellationToken? cancellationToken = null)
		{
			try
			{
				cancellationToken?.ThrowIfCancellationRequested();

				var now = DateTime.Now;
				var dateTime = $"{now.ToShortDateString()} {now.ToShortTimeString()}";
				_out.Write(dateTime);
				_out.Write(' ');
				for (var i = 0; i < (80 - dateTime.Length - 1); i++)
				{
					_out.Write('=');
				}
				_out.WriteLine();

				foreach (var host in _registry)
				{
					foreach (var entry in host.All.Sort())
					{
						_out.Write(entry.Key);
						_out.WriteLine(':');

						foreach (var subEntry in entry.Value)
						{
							_out.Write("  ");
							_out.Write(subEntry.Key);
							_out.WriteLine(':');

							var metric = subEntry.Value;
							switch (metric)
							{
								case GaugeMetric _:
									WriteGauge((GaugeMetric)metric);
									break;
								case CounterMetric _:
									WriteCounter((CounterMetric)metric);
									break;
								case HistogramMetric _:
									WriteHistogram((HistogramMetric)metric);
									break;
								case MeterMetric _:
									WriteMetered((MeterMetric)metric);
									break;
								case TimerMetric _:
									WriteTimer((TimerMetric)metric);
									break;
							}
							_out.WriteLine();
						}
						_out.WriteLine();
						_out.Flush();
					}
				}
			}
			catch (Exception e)
			{
				_out.WriteLine(e.StackTrace);
			}
		}

		public async Task OnTimer(Action action, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(_interval, cancellationToken);

				if (!cancellationToken.IsCancellationRequested)
					action();
			}
		}

		void WriteGauge(GaugeMetric gauge)
		{
			_out.Write("    value = ");
			_out.WriteLine(gauge.ValueAsString);
		}

		void WriteCounter(CounterMetric counter)
		{
			_out.Write("    count = ");
			_out.WriteLine(counter.Count);
		}

		void WriteMetered(IMetered meter)
		{
			var unit = Abbreviate(meter.RateUnit);
			_out.Write("             count = {0}\n", meter.Count);
			_out.Write("         mean rate = {0} {1}/{2}\n", meter.MeanRate, meter.EventType, unit);
			_out.Write("     1-minute rate = {0} {1}/{2}\n", meter.OneMinuteRate, meter.EventType, unit);
			_out.Write("     5-minute rate = {0} {1}/{2}\n", meter.FiveMinuteRate, meter.EventType, unit);
			_out.Write("    15-minute rate = {0} {1}/{2}\n", meter.FifteenMinuteRate, meter.EventType, unit);
		}

		void WriteHistogram(HistogramMetric histogram)
		{
			var percentiles = histogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

			_out.Write("               min = %{0:2}\n", histogram.Min);
			_out.Write("               max = %{0:2}\n", histogram.Max);
			_out.Write("              mean = %{0:2}\n", histogram.Mean);
			_out.Write("            stddev = %{0:2}\n", histogram.StdDev);
			_out.Write("            median = %{0:2}\n", percentiles[0]);
			_out.Write("              75%% <= %{0:2}\n", percentiles[1]);
			_out.Write("              95%% <= %{0:2}\n", percentiles[2]);
			_out.Write("              98%% <= %{0:2}\n", percentiles[3]);
			_out.Write("              99%% <= %{0:2}\n", percentiles[4]);
			_out.Write("            99.9%% <= %{0:2}\n", percentiles[5]);
		}

		void WriteTimer(TimerMetric timer)
		{
			WriteMetered(timer);

			var durationUnit = Abbreviate(timer.DurationUnit);

			var percentiles = timer.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);

			_out.Write("               min = %{0:2}{1}\n", timer.Min, durationUnit);
			_out.Write("               max = %{0:2}{1}\n", timer.Max, durationUnit);
			_out.Write("              mean = %{0:2}{1}\n", timer.Mean, durationUnit);
			_out.Write("            stddev = %{0:2}{1}\n", timer.StdDev, durationUnit);
			_out.Write("            median = %{0:2}{1}\n", percentiles[0], durationUnit);
			_out.Write("              75%% <= %{0:2}{1}\n", percentiles[1], durationUnit);
			_out.Write("              95%% <= %{0:2}{1}\n", percentiles[2], durationUnit);
			_out.Write("              98%% <= %{0:2}{1}\n", percentiles[3], durationUnit);
			_out.Write("              99%% <= %{0:2}{1}\n", percentiles[4], durationUnit);
			_out.Write("            99.9%% <= %{0:2}{1}\n", percentiles[5], durationUnit);
		}

		static string Abbreviate(TimeUnit unit)
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

		public void Dispose()
		{
			Stop();
		}
	}
}

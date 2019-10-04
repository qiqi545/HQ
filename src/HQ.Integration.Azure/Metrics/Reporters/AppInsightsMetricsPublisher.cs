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
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Metrics;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Constants = HQ.Extensions.Metrics.Constants;

namespace HQ.Integration.Azure.Metrics.Reporters
{
	// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.2

	internal class AppInsightsMetricsPublisher : IHealthCheckPublisher
	{
		private readonly TelemetryClient _client;
		private readonly IMetricsRegistry _registry;
		private readonly IOptionsMonitor<AppInsightsMetricsReporterOptions> _reporterOptions;

		public AppInsightsMetricsPublisher(IMetricsRegistry registry, TelemetryClient client,
			IOptionsMonitor<AppInsightsMetricsReporterOptions> reporterOptions)
		{
			_registry = registry;
			_client = client;
			_reporterOptions = reporterOptions;
		}

		public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			var options = _reporterOptions.CurrentValue;

			if (!options.Enabled || cancellationToken.IsCancellationRequested && cancellationToken.CanBeCanceled)
			{
				return Task.CompletedTask;
			}

			var publishHealthy = options.PublishHealthy;

			if (report.Status == HealthStatus.Healthy && publishHealthy)
			{
				return Task.CompletedTask;
			}

			if (options.PublishHealthChecks)
			{
				foreach (var entry in report.Entries)
				{
					if (entry.Value.Status == HealthStatus.Healthy && !publishHealthy)
					{
						continue;
					}

					// https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics

					var properties = new Dictionary<string, string>
					{
						{nameof(entry.Key), entry.Key},
						{nameof(entry.Value.Description), entry.Value.Description},
						{nameof(Environment.MachineName), Environment.MachineName}
					};

					foreach (var item in entry.Value.Data)
					{
						properties[item.Key] = $"{item.Value}";
					}

					if (entry.Value.Exception != null)
					{
						properties[nameof(entry.Value.Exception)] = $"{entry.Value.Exception}";
					}

					var metrics = new Dictionary<string, double>
					{
						{nameof(entry.Value.Status), entry.Value.Status == HealthStatus.Healthy ? 1 : 0},
						{nameof(entry.Value.Duration), entry.Value.Duration.TotalMilliseconds}
					};

					_client.TrackEvent(options.HealthCheckEventName, properties, metrics);
				}
			}

			if (options.PublishMetrics)
			{
				// https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics

				foreach (var registry in _registry.Manifest)
				{
					var properties = new Dictionary<string, string> {{"Key", registry.Key}};

					var hostType = registry.Value.GetType();
					if (hostType.IsGenericType)
					{
						properties["Scope"] = $"{hostType.GetGenericArguments()[0].Name}";
					}

					var sample = registry.Value.GetSample();

					var metrics = new Dictionary<string, double>();

					foreach (var entry in sample)
					{
						var metricName = entry.Key.Name;

						if (report.Entries.ContainsKey($"health_check.{metricName}"))
						{
							continue; // was already reported as a health check
						}

						switch (entry.Value)
						{
							case GaugeMetric gauge:
								var gaugeType = gauge.GetType();
								if (!gaugeType.IsGenericType)
								{
									properties.Add(metricName, gauge.ValueAsString);
								}
								else
								{
									var dataType = gaugeType.GetGenericArguments()[0];
									if (dataType.IsNumeric())
									{
										metrics.Add(metricName, double.Parse(gauge.ValueAsString));
									}
									else
									{
										properties.Add(metricName, gauge.ValueAsString);
									}
								}

								break;
							case CounterMetric counter:
								metrics.Add(metricName, counter.Count);
								break;
							case HistogramMetric histogram:
								AddDistributed(metricName, histogram, metrics);
								break;
							case MeterMetric meter:
								AddMetered(metricName, meter, properties, metrics);
								break;
							case TimerMetric timer:
								AddDistributed(metricName, timer, metrics);
								AddMetered(metricName, timer, properties, metrics);
								break;
						}
					}

					if (metrics.Count > 0)
						_client.TrackEvent(options.MetricsSampleEventName, properties, metrics);
				}
			}

			return Task.CompletedTask;

			void AddDistributed(string metricName, IDistributed distributed, IDictionary<string, double> metrics)
			{
				if (!metrics.ContainsKey($"{metricName}.{nameof(IDistributed.Count)}"))
				{
					metrics.Add($"{metricName}.{nameof(IDistributed.Count)}", distributed.Count);
				}

				metrics.Add($"{metricName}.{nameof(IDistributed.Max)}", distributed.Max);
				metrics.Add($"{metricName}.{nameof(IDistributed.Mean)}", distributed.Mean);
				metrics.Add($"{metricName}.{nameof(IDistributed.Min)}", distributed.Min);
				metrics.Add($"{metricName}.{nameof(IDistributed.StdDev)}", distributed.StdDev);

				var bands = Constants.Percentiles;
				var percentiles = distributed.Percentiles(bands);

				metrics.Add($"{metricName}.%{bands[0]:2}", percentiles[0]);
				metrics.Add($"{metricName}.%{bands[1]:2}", percentiles[1]);
				metrics.Add($"{metricName}.%{bands[2]:2}", percentiles[2]);
				metrics.Add($"{metricName}.%{bands[3]:2}", percentiles[3]);
				metrics.Add($"{metricName}.%{bands[4]:2}", percentiles[4]);
			}

			void AddMetered(string metricName, IMetered meter, IDictionary<string, string> properties,
				IDictionary<string, double> metrics)
			{
				properties.Add($"{metricName}.{nameof(IMetered.EventType)}", meter.EventType);
				properties.Add($"{metricName}.{nameof(IMetered.RateUnit)}", $"{meter.RateUnit}");

				if (!metrics.ContainsKey("{metricName}.{nameof(IMetered.Count)}"))
				{
					metrics.Add($"{metricName}.{nameof(IMetered.Count)}", meter.Count);
				}

				metrics.Add($"{metricName}.{nameof(IMetered.FifteenMinuteRate)}", meter.FifteenMinuteRate);
				metrics.Add($"{metricName}.{nameof(IMetered.FiveMinuteRate)}", meter.FiveMinuteRate);
				metrics.Add($"{metricName}.{nameof(IMetered.MeanRate)}", meter.MeanRate);
				metrics.Add($"{metricName}.{nameof(IMetered.OneMinuteRate)}", meter.OneMinuteRate);
			}
		}
	}
}
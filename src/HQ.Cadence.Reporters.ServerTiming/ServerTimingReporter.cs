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
using System.Threading.Tasks;
using HQ.Cadence.Internal;
using HQ.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Cadence.Reporters.ServerTiming
{
    public class ServerTimingReporter : IMetricsReporter
    {
        internal static IApplicationBuilder AppBuilder { get; set; }

        public Task InitializeAsync()
        {
            AppBuilder.Use(async (context, next) =>
            {
                var options = context.RequestServices.GetService<IOptions<ServerTimingReporterOptions>>();
                if (options?.Value != null && options.Value.Enabled)
                {
                    context.Response.Headers[Constants.HttpHeaders.TimingAllowOrigin] = options.Value.AllowedOrigins;

                    context.Response.OnStarting(() =>
                    {
                        var metrics = context.RequestServices.GetService<IMetricsRegistry>();
                        if (metrics != null)
                            AddMetricsToServerTiming(metrics, context);

                        var healthChecks = context.RequestServices.GetService<IHealthChecksRegistry>();
                        if (healthChecks != null)
                            AddMetricsToServerTiming(healthChecks, context);

                        return Task.CompletedTask;
                    });
                }

                await next();
            });

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        private static void AddMetricsToServerTiming(IEnumerable<IReadableMetrics> metrics, HttpContext context)
        {
            foreach (var host in metrics)
            {
                foreach (var entry in host.AsReadOnly().Sort())
                {
                    foreach (var subEntry in entry.Value)
                    {
                        AddMetricValue(context, subEntry, entry);
                    }
                }
            }
        }

        private static void AddMetricValue(HttpContext context, KeyValuePair<string, IMetric> subEntry,
            KeyValuePair<string, IDictionary<string, IMetric>> entry)
        {
            var metric = subEntry.Value;
            var key = $"{entry.Key}.{subEntry.Key}";

            switch (metric)
            {
                case GaugeMetric x:
                {
                    if (x.IsNumeric)
                    {
                        var value = $"{key};dur={x.ValueAsString}";
                        AddServerTimingEntry(context, value);
                    }
                    else if (x.IsBoolean && x.ValueAsString.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = $"{key}";
                        AddServerTimingEntry(context, value);
                    }

                    break;
                }
                case CounterMetric x:
                {
                    var value = $"{key};dur={x.Count}";
                    AddServerTimingEntry(context, value);
                    break;
                }
                case HistogramMetric x:
                {
                    WriteDistributed(x, key, context);
                    break;
                }
                case MeterMetric x:
                {
                    WriteMetered(x, key, context);
                    break;
                }
                case TimerMetric x:
                {
                    WriteMetered(x, key, context);
                    WriteDistributed(x, key, context);
                    break;
                }
            }
        }

        private static void WriteDistributed(IDistributed x, string key, HttpContext context)
        {
            var percentiles = x.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);
            var max = $"{key}.max;dur={x.Max}";
            var min = $"{key}.min;dur={x.Min}";
            var mean = $"{key}.mean;dur={x.Mean}";
            var stdDev = $"{key}.stdDev;dur={x.StdDev}";
            var median = $"{key}.median;dur={percentiles[0]}";

            var desc = x is TimerMetric timer ? $" <= {timer.DurationUnit.Abbreviate()}\"" : string.Empty;

            var p75 = $"{key}.p75;dur={percentiles[1]};desc=\"75%%{desc}\"";
            var p95 = $"{key}.p95;dur={percentiles[2]};desc=\"95%%{desc}\"";
            var p98 = $"{key}.p98;dur={percentiles[3]};desc=\"98%%{desc}\"";
            var p99 = $"{key}.p99;dur={percentiles[4]};desc=\"99%%{desc}\"";
            var p999 = $"{key}.p999;dur={percentiles[5]};desc=\"99.99%%{desc}\"";

            AddServerTimingEntry(context, max);
            AddServerTimingEntry(context, min);
            AddServerTimingEntry(context, mean);
            AddServerTimingEntry(context, stdDev);
            AddServerTimingEntry(context, median);

            AddServerTimingEntry(context, p75);
            AddServerTimingEntry(context, p95);
            AddServerTimingEntry(context, p98);
            AddServerTimingEntry(context, p99);
            AddServerTimingEntry(context, p999);
        }

        private static void WriteMetered(IMetered x, string key, HttpContext context)
        {
            var per = $"{x.EventType}/{x.RateUnit.Abbreviate()}";

            var count = $"{key}.count;dur={x.Count};desc=\"{per}\"";
            var meanRate = $"{key}.meanRate;dur={x.MeanRate};desc=\"{per}\"";
            var one = $"{key};dur={x.OneMinuteRate};desc=\"{per}\"";
            var five = $"{key};dur={x.FiveMinuteRate};desc=\"{per}\"";
            var fifteen = $"{key};dur={x.FifteenMinuteRate};desc=\"{per}\"";

            AddServerTimingEntry(context, count);
            AddServerTimingEntry(context, meanRate);
            AddServerTimingEntry(context, one);
            AddServerTimingEntry(context, five);
            AddServerTimingEntry(context, fifteen);
        }

        private static void AddServerTimingEntry(HttpContext context, string value)
        {
            context.Response.Headers.Add(Constants.HttpHeaders.ServerTiming, value);
        }
    }
}

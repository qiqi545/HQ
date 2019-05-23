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
using System.Collections.Immutable;
using System.Text;
using HQ.Common;

namespace HQ.Extensions.Metrics.Internal
{
    public class JsonSampleSerializer
    {
        public static Func<IImmutableDictionary<MetricName, IMetric>, string> Serialize = metrics =>
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append('[');

                foreach (var metric in metrics)
                {
                    if (metric.Key.Class != null && metric.Key.Class != typeof(MetricsHost))
                    {
                        sb.Append("{\"scope\":\"").Append(metric.Key.Class.Name).Append("\",");
                    }
                    else
                    {
                        sb.Append('{');
                    }

                    sb.Append("\"name\":\"").Append(metric.Key.Name).Append("\",\"metric\":");

                    switch (metric.Value)
                    {
                        case CounterMetric counter:
                            AppendCounter(sb, counter);
                            break;
                        case GaugeMetric gauge:
                            AppendGauge(sb, gauge);
                            break;
                        case HistogramMetric histogram:
                            AppendHistogram(sb, histogram);
                            break;
                        case MeterMetric meter:
                            AppendMeter(sb, meter);
                            break;
                        case TimerMetric timer:
                            AppendTimer(sb, timer);
                            break;
                    }

                    sb.Append("},");
                }

                if (metrics.Count > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                sb.Append(']');
            });
        };

        private static void AppendMeter(StringBuilder sb, IMetered meter)
        {
            sb.Append("{\"count\":").Append(meter.Count)
                .Append(",\"rate_unit\":\"").Append(meter.RateUnit).Append("\"")
                .Append(",\"fifteen_minute_rate\":").Append(meter.FifteenMinuteRate)
                .Append(",\"five_minute_rate\":").Append(meter.FiveMinuteRate)
                .Append(",\"one_minute_rate\":").Append(meter.OneMinuteRate)
                .Append(",\"mean_rate\":").Append(meter.MeanRate).Append("}");
        }

        private static void AppendHistogram(StringBuilder sb, IDistributed histogram)
        {
            sb.Append('{');
            AppendDistribution(sb, histogram);
            sb.Append('}');
        }

        private static void AppendDistribution(StringBuilder sb, IDistributed distribution)
        {
            sb.Append("\"count\":").Append(distribution.Count)
                .Append(",\"max\":").Append(distribution.Max)
                .Append(",\"min\":").Append(distribution.Min)
                .Append(",\"mean\":").Append(distribution.Mean)
                .Append(",\"std_dev\":").Append(distribution.StdDev);

            sb.Append(",\"percentiles\":[");
            var values = distribution.Percentiles(Constants.Percentiles);
            for (var i = 0; i < values.Length; i++)
            {
                sb.Append(values[i]);
                if (i < values.Length - 1)
                {
                    sb.Append(',');
                }
            }

            sb.Append(']');
        }

        private static void AppendTimer(StringBuilder sb, TimerMetric timer)
        {
            sb.Append('{');

            AppendDistribution(sb, timer);

            sb.Append(",\"duration_unit\":\"").Append(timer.DurationUnit).Append("\"")
                .Append(",\"rate_unit\":\"").Append(timer.RateUnit).Append("\"")
                .Append(",\"fifteen_minute_rate\":").Append(timer.FifteenMinuteRate)
                .Append(",\"five_minute_rate\":").Append(timer.FiveMinuteRate)
                .Append(",\"one_minute_rate\":").Append(timer.OneMinuteRate)
                .Append(",\"mean_rate\":").Append(timer.MeanRate);

            sb.Append('}');
        }

        private static void AppendGauge(StringBuilder sb, GaugeMetric gauge)
        {
            sb.Append("{\"value\":").Append(gauge.ValueAsString).Append('}');
        }

        private static void AppendCounter(StringBuilder sb, CounterMetric counter)
        {
            sb.Append("{\"count\":").Append(counter.Count).Append('}');
        }
    }
}

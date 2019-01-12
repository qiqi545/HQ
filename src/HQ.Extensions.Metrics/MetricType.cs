using System;

namespace HQ.Extensions.Metrics
{
    [Flags]
    public enum MetricType
    {
        Gauge      = (byte)1u << 0,
        Counter    = (byte)1u << 1,
        Meter      = (byte)1u << 2,
        Histogram  = (byte)1u << 3,
        Timer      = (byte)1u << 4,

        None = 0,
        All = 0xFF
    }

    public static class MetricTypeExtensions
    {
        public static bool HasFlagFast(this MetricType value, MetricType flag)
        {
            return (value & flag) != 0;
        }
    }
}

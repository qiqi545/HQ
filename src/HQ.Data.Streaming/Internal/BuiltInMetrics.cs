using HQ.Data.Streaming.Memory;
using Metrics;

namespace HQ.Data.Streaming.Internal
{
	internal static class BuiltInMetrics
	{
		public static void BytesPerSecond(IMetricsHost metrics, int length)
		{
			metrics?.Meter(typeof(LineReader), "bytes_read_per_second", "bytes", TimeUnit.Seconds).Mark(length);
		}

		public static void LineLength<T>(IMetricsHost metrics, int length)
		{
			metrics?.Histogram(typeof(FileMemoryProvider<T>), "line_length", SampleType.Uniform).Update(length);
		}

		public static double GetMeanLineLength<T>(IMetricsHost metrics)
		{
			return metrics?.Histogram(typeof(FileMemoryProvider<T>), "line_length", SampleType.Uniform).Mean ?? 0;
		}
	}
}
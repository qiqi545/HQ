using BenchmarkDotNet.Running;

namespace HQ.Extensions.Messaging.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<MessageAggregationBenchmarks>();
		}
	}
}

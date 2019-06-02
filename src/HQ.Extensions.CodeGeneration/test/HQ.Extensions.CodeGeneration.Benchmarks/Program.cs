using BenchmarkDotNet.Running;

namespace HQ.Extensions.CodeGeneration.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<ObjectActivationBenchmarks>();
		}
	}
}

using System;
using BenchmarkDotNet.Running;

namespace HQ.Extensions.DependencyInjection.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<ResolveInstancedSingletonBenchmark>();
		}
	}
}

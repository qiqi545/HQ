using System;
using BenchmarkDotNet.Running;

namespace HQ.Harmony.Benchmarks
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<ResolveBenchmark>();
		}
	}
}

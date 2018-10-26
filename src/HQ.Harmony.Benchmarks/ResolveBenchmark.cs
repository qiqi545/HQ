using Autofac;
using BenchmarkDotNet.Attributes;

namespace HQ.Harmony.Benchmarks
{
	[CoreJob, MarkdownExporter]
	public class ResolveBenchmark
	{
		[Params(100000)]
		public int trials;

		#region Containers

		private IContainer _harmony;
		private Autofac.IContainer _autofac;

		#endregion

		[GlobalSetup]
		public void SetUp()
		{
			_harmony = new HarmonyContainer();
			_harmony.Register(new Singleton());

			var builder = new ContainerBuilder();
			builder.RegisterInstance(new Singleton());
			_autofac = builder.Build();
		}

		#region Surrogates

		public class Singleton { }

		#endregion

		[Benchmark]
		public void Resolve_singleton_Harmony()
		{
			for (var i = 0; i < trials; i++)
			{
				_harmony.Resolve<Singleton>();
			}
		}

		[Benchmark]
		public void Resolve_singleton_Autofac()
		{
			for (var i = 0; i < trials; i++)
			{
				_autofac.Resolve<Singleton>();
			}
		}
	}
}
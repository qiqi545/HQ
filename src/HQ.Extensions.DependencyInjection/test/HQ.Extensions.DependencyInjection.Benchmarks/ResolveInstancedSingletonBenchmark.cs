using System.Reflection;
using Autofac;
using BenchmarkDotNet.Attributes;
using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Ninject;

namespace HQ.Extensions.DependencyInjection.Benchmarks
{
	[CoreJob, MarkdownExporter, MemoryDiagnoser]
	public class ResolveInstancedSingletonBenchmark
	{
		[Params(100000)] public int Trials;

		#region Containers

		private IContainer _harmony;
		private Autofac.IContainer _autofac;
		private IKernel _ninject;
		private DryIoc.IContainer _dry;
		private ServiceProvider _dotnet;
		private Singleton _native;

		#endregion

		[GlobalSetup]
		public void SetUp()
		{
			_harmony = new DependencyContainer();
			_harmony.Register(new Singleton());
			_harmony.Resolve<Singleton>();

			var builder = new ContainerBuilder();
			builder.RegisterInstance(new Singleton());
			_autofac = builder.Build();
			_autofac.Resolve<Singleton>();

			_ninject = new StandardKernel();
			_ninject.Load(Assembly.GetExecutingAssembly());
			_ninject.Bind<Singleton>().ToConstant(new Singleton());
			_ninject.Get<Singleton>();

			_dry = new Container();
			_dry.Register<Singleton>(Reuse.Singleton);
			_dry.Resolve<Singleton>();

			var services = new ServiceCollection();
			services.AddSingleton(new Singleton());
			_dotnet = services.BuildServiceProvider();
			_dotnet.GetService<Singleton>();

			_native = new Singleton();
		}

		#region Surrogates

		public class Singleton
		{
		}

		#endregion

		[Benchmark]
		public void Resolve_singleton_Harmony()
		{
			for (var i = 0; i < Trials; i++)
			{
				_harmony.Resolve<Singleton>();
			}
		}

		[Benchmark]
		public void Resolve_singleton_Autofac()
		{
			for (var i = 0; i < Trials; i++)
			{
				_autofac.Resolve<Singleton>();
			}
		}

		[Benchmark]
		public void Resolve_singleton_Ninject()
		{
			for (var i = 0; i < Trials; i++)
			{
				_ninject.Get<Singleton>();
			}
		}

		[Benchmark]
		public void Resolve_singleton_Dry()
		{
			for (var i = 0; i < Trials; i++)
			{
				_dry.Resolve<Singleton>();
			}
		}

		[Benchmark]
		public void Resolve_singleton_DotNet()
		{
			for (var i = 0; i < Trials; i++)
			{
				_dotnet.GetService<Singleton>();
			}
		}

		[Benchmark]
		public void Resolve_singleton_native()
		{
			for (var i = 0; i < Trials; i++)
			{
				var instance = _native;
			}
		}
	}
}

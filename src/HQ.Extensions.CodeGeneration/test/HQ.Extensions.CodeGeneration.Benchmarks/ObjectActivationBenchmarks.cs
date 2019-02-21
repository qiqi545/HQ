using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace HQ.Extensions.CodeGeneration.Benchmarks
{
	[CoreJob, MarkdownExporter, MemoryDiagnoser]
	public class ObjectActivationBenchmarks
	{
		[Params(100000)] public int trials;

		[GlobalSetup]
		public void SetUp()
		{
			InstanceFactory.Default.CreateInstance<Class>();
		}

		#region Factories

		private static class ActivatorFactory
		{
			public static InstanceFactory.ObjectActivator Build(Type implementationType)
			{
				return objects => Activator.CreateInstance(implementationType, objects);
			}
		}

		private static class ConstructorInvokeFactory
		{
			public static InstanceFactory.ObjectActivator Build(ConstructorInfo ctor)
			{
				return ctor.Invoke;
			}
		}

		#endregion

		#region Surrogates

		public class Class { }

		#endregion

		[Benchmark]
		public void ParameterlessConstructor_Native()
		{
			var activator = (InstanceFactory.ObjectActivator)(parameters => new Class());
			for (var i = 0; i < trials; i++)
			{
				activator();
			}
		}

		[Benchmark]
		public void ParameterlessConstructor_Activator()
		{
			var activator = ActivatorFactory.Build(typeof(Class));
			for (var i = 0; i < trials; i++)
			{
				activator();
			}
		}

		[Benchmark]
		public void ParameterlessConstructor_ConstructorInvoke()
		{
			var ctor = typeof(Class).GetConstructor(Type.EmptyTypes);
			var activator = ConstructorInvokeFactory.Build(ctor);
			for (var i = 0; i < trials; i++)
			{
				activator();
			}
		}

		[Benchmark]
		public void ParameterlessConstructor_CompiledExpression()
		{
			var ctor = typeof(Class).GetConstructor(Type.EmptyTypes);
			var activator = InstanceFactory.CompiledExpressionFactory.Build(ctor);
			for (var i = 0; i < trials; i++)
			{
				activator();
			}
		}

		[Benchmark]
		public void ParameterlessConstructor_DynamicMethod()
		{
			var ctor = typeof(Class).GetConstructor(Type.EmptyTypes);
			var activator = InstanceFactory.DynamicMethodFactory.Build(typeof(Class), ctor);
			for (var i = 0; i < trials; i++)
			{
				activator();
			}
		}
	}
}

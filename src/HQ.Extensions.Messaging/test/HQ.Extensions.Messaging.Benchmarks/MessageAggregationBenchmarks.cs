using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using HQ.Extensions.Messaging.Bus;
using reactive.pipes;

namespace HQ.Extensions.Messaging.Benchmarks
{
	[CoreJob, MarkdownExporter, MemoryDiagnoser]
	public class MessageAggregationBenchmarks
	{
		[Params(10000)]
		public int trials;

		public class BaseMessage { }

		public class SubMessage : BaseMessage { }

		public class DoNothingHandler : IConsume<BaseMessage>, IMessageHandler<BaseMessage>
		{
			public Task<bool> HandleAsync(BaseMessage message)
			{
				return Task.FromResult(true);
			}

			public bool Handle(BaseMessage message)
			{
				return true;
			}
		}

		public class ManifoldHandler : IConsume<BaseMessage>, IConsume<SubMessage>, IMessageHandler<BaseMessage>, IMessageHandler<SubMessage>
		{
			public Task<bool> HandleAsync(BaseMessage message)
			{
				return Task.FromResult(true);
			}

			public Task<bool> HandleAsync(SubMessage message)
			{
				return Task.FromResult(true);
			}

			public bool Handle(BaseMessage message)
			{
				return true;
			}

			public bool Handle(SubMessage message)
			{
				return true;
			}
		}

		[Benchmark]
		public void ReactivePipes_single_type()
		{
			var hub = new Hub();
			hub.Subscribe(new DoNothingHandler());
			for (var i = 0; i < trials; i++)
			{
				hub.Publish(new BaseMessage());
			}
		}

		[Benchmark]
		public void ReactivePipes_manifold_type()
		{
			var hub = new Hub();
			hub.Subscribe(new ManifoldHandler());
			for (var i = 0; i < trials; i++)
			{
				hub.Publish(new SubMessage());
			}
		}

		[Benchmark]
		public void Flow_single_type()
		{
			var aggregator = new MessageAggregator();
			var publisher = new MessagePublisher(aggregator);
			aggregator.Subscribe(new DoNothingHandler());
			for (var i = 0; i < trials; i++)
			{
				publisher.Publish(new BaseMessage());
			}
		}

		[Benchmark]
		public void Flow_manifold_type()
		{
			var aggregator = new MessageAggregator();
			var publisher = new MessagePublisher(aggregator);
			aggregator.Subscribe(new ManifoldHandler());
			for (var i = 0; i < trials; i++)
			{
				publisher.Publish(new SubMessage());
			}
		}
	}
}
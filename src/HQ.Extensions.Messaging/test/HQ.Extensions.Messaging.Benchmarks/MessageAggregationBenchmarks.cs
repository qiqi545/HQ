#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using HQ.Extensions.Messaging.Bus;
using reactive.pipes;

namespace HQ.Extensions.Messaging.Benchmarks
{
    [CoreJob]
    [MarkdownExporter]
    [MemoryDiagnoser]
    public class MessageAggregationBenchmarks
    {
        [Params(10000)] public int trials;

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

        public class BaseMessage
        {
        }

        public class SubMessage : BaseMessage
        {
        }

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

        public class ManifoldHandler : IConsume<BaseMessage>, IConsume<SubMessage>, IMessageHandler<BaseMessage>,
            IMessageHandler<SubMessage>
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
    }
}

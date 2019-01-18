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

using System;
using HQ.Extensions.Messaging.Bus;
using HQ.Extensions.Messaging.Tests.Bus.Handlers;
using HQ.Extensions.Messaging.Tests.Bus.Messages;
using Xunit;

namespace HQ.Extensions.Messaging.Tests.Bus
{
    public class MessageAggregatorTests
    {
        [Fact]
        public void Can_subscribe_with_manifold_hierarchical_consumer()
        {
            var aggregator = new MessageAggregator();
            var publisher = new MessagePublisher(aggregator);

            var handler = new ManifoldHierarchicalMessageHandler();
            aggregator.Subscribe(handler);

            var sent = publisher.Publish(new InheritedMessage());
            Assert.True(sent);
            Assert.Equal(1, handler.HandledInterface);
            Assert.Equal(1, handler.HandledBase);
            Assert.Equal(1, handler.HandledInherited);
        }

        [Fact]
        public void Can_subscribe_with_multiple_hierarchical_consumers()
        {
            var hub = new MessageHub();

            var handler1 = new ManifoldHierarchicalMessageHandler();
            var handler2 = new ManifoldHierarchicalMessageHandler();
            var handler3 = new ManifoldHierarchicalMessageHandler();

            hub.Subscribe<IMessage>(handler1);
            hub.Subscribe<BaseMessage>(handler2);
            hub.Subscribe<InheritedMessage>(handler3);

            var sent = hub.Publish(new InheritedMessage());
            Assert.True(sent);

            Assert.Equal(1, handler1.HandledInterface);
            Assert.Equal(0, handler1.HandledBase);
            Assert.Equal(0, handler1.HandledInherited);

            Assert.Equal(0, handler2.HandledInterface);
            Assert.Equal(1, handler2.HandledBase);
            Assert.Equal(0, handler2.HandledInherited);

            Assert.Equal(0, handler3.HandledInterface);
            Assert.Equal(0, handler3.HandledBase);
            Assert.Equal(1, handler3.HandledInherited);
        }

        [Fact]
        public void Handlers_can_return_false_safely()
        {
            var handler = new FailingHandler();
            var hub = new MessageHub();
            hub.Subscribe(handler);

            var result = hub.Publish(new InheritedMessage());
            Assert.Equal(1, handler.Handled);
            Assert.False(result);
        }

        [Fact]
        public void Handlers_survive_exceptions()
        {
            var handler = new ThrowingHandler();
            var hub = new MessageHub();
            hub.Subscribe(handler);

            hub.Publish(new InheritedMessage());
            Assert.Equal(1, handler.Handled);

            hub.Publish(new InheritedMessage());
            Assert.Equal(2, handler.Handled);
        }

        [Fact]
        public void Multiple_subscriptions_of_the_same_kind_dont_duplicate()
        {
            var handler1 = new StringMessageHandler();
            var handler2 = new StringMessageHandler();
            var handled = 0;

            var aggregator = new MessageHub();
            aggregator.Subscribe(handler1);
            aggregator.Subscribe<StringMessage>(e => handled++);
            aggregator.Subscribe(handler2);

            var sent = aggregator.Publish(new StringMessage {Text = "Foo"});
            Assert.True(sent);
            Assert.Equal(1, handler1.Handled);
            Assert.Equal(1, handled);
            Assert.Equal(1, handler2.Handled);
        }

        [Fact]
        public void Publishes_to_handler()
        {
            var aggregator = new MessageAggregator();
            var publisher = new MessagePublisher(aggregator);

            var handled = 0;
            aggregator.Subscribe<StringMessage>(se => { handled++; });

            var sent = publisher.Publish(new StringMessage("Foo"));
            Assert.True(sent);
            Assert.Equal(1, handled);
        }

        [Fact]
        public void Publishes_to_manifold_handler()
        {
            var aggregator = new MessageAggregator();
            var publisher = new MessagePublisher(aggregator);

            var handler = new ManifoldMessageHandler();
            aggregator.Subscribe(handler);

            var sent = publisher.Publish(new StringMessage("Foo"));
            Assert.True(sent);
            Assert.Equal(1, handler.HandledString);
            Assert.Equal(0, handler.HandledInteger);

            sent = publisher.Publish(new IntegerMessage(123));
            Assert.True(sent);
            Assert.Equal(1, handler.HandledString);
            Assert.Equal(1, handler.HandledInteger);
        }

        [Fact]
        public void Publishes_to_multicast_handlers()
        {
            var hub = new MessageHub();

            var baseCalled = 0;
            var inheritedCalled = 0;

            void Handler1(BaseMessage e)
            {
                baseCalled++;
            }

            void Handler2(InheritedMessage e)
            {
                inheritedCalled++;
            }

            hub.Subscribe((Action<BaseMessage>) Handler1);
            hub.Subscribe((Action<InheritedMessage>) Handler2);

            // one handler, many events (by virtue of class hierarchy)
            var sent = hub.Publish(new InheritedMessage {Id = 123, Value = "ABC"});
            Assert.True(sent);
            Assert.Equal(1, inheritedCalled);
            Assert.Equal(1, baseCalled);
        }

        [Fact]
        public void Publishes_to_multicast_handlers_with_interfaces_with_concrete_consumer()
        {
            var handler = new IncrementingHandler();
            var hub = new MessageHub();
            hub.Subscribe(handler);
            hub.Publish(new InheritedMessage());
            Assert.Equal(1, handler.Handled);
        }

        [Fact]
        public void Publishes_to_multicast_handlers_with_interfaces_with_delegate_consumer()
        {
            var handled = 0;

            var hub = new MessageHub();
            void Handler(IMessage e) => handled++;
            hub.Subscribe((Action<IMessage>) Handler);

            var sent = hub.Publish(new InheritedMessage {Id = 123, Value = "ABC"});
            Assert.True(sent, "did not send event to a known subscription");
            Assert.Equal(1, handled);
        }

        [Fact]
        public void Publishes_to_multicast_handlers_with_no_existing_subscriptions()
        {
            var handled = 0;

            var hub = new MessageHub();
            hub.Subscribe<BaseMessage>(e => handled++);

            var sent = hub.Publish(new InheritedMessage {Id = 123, Value = "ABC"});
            Assert.True(sent, "did not send event to a known subscription");
            Assert.Equal(1, handled);
        }

        [Fact]
        public void Publishes_to_multiple_handlers()
        {
            var aggregator = new MessageAggregator();
            var publisher = new MessagePublisher(aggregator);

            var handler1 = 0;
            var handler2 = 0;

            aggregator.Subscribe<StringMessage>(e => { handler1++; });
            aggregator.Subscribe<StringMessage>(e => { handler2++; });

            var sent = publisher.Publish(new StringMessage("Foo"));
            Assert.True(sent);
            Assert.Equal(1, handler1);
            Assert.Equal(1, handler2);
        }

        [Fact]
        public void Same_consumer_will_throw()
        {
            Assert.Throws<DuplicateSubscriptionException>(() =>
            {
                var handler = new StringMessageHandler();
                var aggregator = new MessageHub();
                aggregator.Subscribe(handler);
                aggregator.Subscribe(handler);
            });
        }

        [Fact]
        public void Two_handlers_for_the_same_event_type()
        {
            var hub = new MessageHub();
            var h1 = new StringMessageHandler();
            var h2 = new StringMessageHandler2();
            hub.Subscribe(h1);
            hub.Subscribe(h2);
            var result = hub.Publish(new StringMessage("value"));
            Assert.True(h1.Handled == 1);
            Assert.True(h2.Handled == 1);
            Assert.True(result);
        }

        [Fact]
        public void Two_identical_closure_handlers()
        {
            var handled1 = 0;
            var handled2 = 0;
            var hub = new MessageHub();
            hub.Subscribe(new Action<StringMessage>(x => handled1++));
            hub.Subscribe(new Action<StringMessage>(x => handled2++));
            var result = hub.Publish(new StringMessage("value"));
            Assert.True(handled1 == 1);
            Assert.True(handled2 == 1);
            Assert.True(result);
        }

        [Fact]
        public void Two_identical_closure_handlers_through_manifold()
        {
            var handled1 = 0;
            var handled2 = 0;
            var hub = new MessageHub();
            hub.Subscribe((object) new Action<StringMessage>(x => handled1++));
            hub.Subscribe((object) new Action<StringMessage>(x => handled2++));
            var result = hub.Publish(new StringMessage("value"));
            Assert.True(handled1 == 1);
            Assert.True(handled2 == 1);
            Assert.True(result);
        }

        [Fact]
        public void Two_identical_handlers()
        {
            var hub = new MessageHub();
            var h1 = new StringMessageHandler();
            var h2 = new StringMessageHandler();
            hub.Subscribe(h1);
            hub.Subscribe(h2);
            var result = hub.Publish(new StringMessage("value"));
            Assert.True(h1.Handled == 1);
            Assert.True(h2.Handled == 1);
            Assert.True(result);
        }
    }
}

// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Flow.Bus;
using HQ.Flow.Tests.Bus.Handlers;
using HQ.Flow.Tests.Bus.Messages;
using Xunit;

namespace HQ.Flow.Tests.Bus
{
	public class MessagePublisherTests
	{
		[Fact]
		public void Can_handle_error_with_callback()
		{
			var errors = 0;

			var hub = new MessageHub();

			var sent = false;

			// SubscribeWithDelegate:
			{
				hub.Subscribe<InheritedMessage>(e => throw new Exception(), (m, ex) => { errors++; });
				object @event = new InheritedMessage {Id = 123, Value = "ABC"};
				sent = hub.Publish(@event);
				Assert.False(sent, "publishing an exception should bubble as false to the publish result");
				Assert.Equal(1, errors);
			}

			// Subscribe (manifold):
			{
				var handler = new ManifoldHierarchicalMessageHandler();
				hub.Subscribe(handler, (m, ex) => { errors++; });
				sent = hub.Publish(new ErrorMessage {Error = true});
				Assert.False(sent);
				Assert.Equal(2, errors);
			}

			// SubscribeWithInterface:
			{
				hub.Subscribe(new ThrowingHandler());
				sent = hub.Publish(new ErrorMessage {Error = true});
				Assert.False(sent);
				Assert.Equal(3, errors);
			}
		}

		[Fact]
		public void Can_publish_events_by_type()
		{
			var handled = 0;
			var hub = new MessageHub();
			hub.Subscribe<InheritedMessage>(e => handled++);
			object @event = new InheritedMessage {Id = 123, Value = "ABC"};
			var sent = hub.Publish(@event);

			Assert.True(sent, "did not send event to a known subscription");
			Assert.Equal(1, handled);
		}

		[Fact]
		public void Multiple_subscriptions_with_different_results_should_be_pessimistic_and_sequential()
		{
			var hub = new MessageHub();
			var pub = (IMessagePublisher) hub;
			var sub = (IMessageAggregator) hub;

			sub.Subscribe(new FailingHandler()); // always false
			sub.Subscribe(new SucceedingHandler()); // always true

			var bad = pub.Publish(new ErrorMessage {Error = true});
			Assert.False(bad);

			var good = pub.Publish(new ErrorMessage {Error = false});
			Assert.False(good);
		}
	}
}
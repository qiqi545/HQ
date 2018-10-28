// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Flow.Bus;
using HQ.Flow.Tests.Handlers;
using HQ.Flow.Tests.Messages;
using Xunit;

namespace HQ.Flow.Tests
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
				object @event = new InheritedMessage { Id = 123, Value = "ABC"};
				sent = hub.Publish(@event);
				Assert.False(sent, "publishing an exception should bubble as false to the publish result");
				Assert.Equal(1, errors);
			}

			// Subscribe (manifold):
			{
				var handler = new ManifoldHierarchicalMessageHandler();
				hub.Subscribe(handler, (m, ex) => { errors++; });
				sent = hub.Publish(new ErrorMessage { Error = true });
				Assert.False(sent);
				Assert.Equal(3, errors);
			}

			// SubscribeWithInterface:
			{
				hub.Subscribe(new ThrowingHandler());
				sent = hub.Publish(new ErrorMessage { Error = true });
				Assert.False(sent);
				Assert.Equal(4, errors);
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

			// two handlers for the same event
			sub.Subscribe(new FailingHandler()); // false
			sub.Subscribe(new SucceedingHandler()); // true

			// and one command to rule them all
			var two = pub.Publish(new ErrorMessage {Error = false});
			var one = pub.Publish(new ErrorMessage {Error = true});

			var bad = one;
			Assert.False(bad, "whoops, outcomes are optimistic"); // <-- pessimistic

			// now, send another message, because we have to ensure outcomes are cleared!
			var good = two;
			Assert.True(good, "whoops, outcomes are broken"); // <-- idempotent outcomes
		}
	}
}
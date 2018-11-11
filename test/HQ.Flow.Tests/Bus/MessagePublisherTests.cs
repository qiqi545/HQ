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

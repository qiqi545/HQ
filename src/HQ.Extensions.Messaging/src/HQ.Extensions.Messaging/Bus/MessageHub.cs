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

namespace HQ.Extensions.Messaging.Bus
{
    public sealed class MessageHub : IMessageAggregator, IMessagePublisher
    {
        private readonly IMessageAggregator _aggregator;
        private readonly IMessagePublisher _publisher;

        public MessageHub()
        {
            _aggregator = new MessageAggregator();
            _publisher = new MessagePublisher(_aggregator);
        }

        public MessageHub(IMessageAggregator aggregator, IMessagePublisher publisher)
        {
            _aggregator = aggregator;
            _publisher = publisher;
        }

        public void Subscribe(object handler, Action<object, Exception> onError = null)
        {
            _aggregator.Subscribe(handler, onError);
        }

        public void Subscribe<T>(IMessageHandler<T> handler, Action<T, Exception> onError = null) where T : class
        {
            _aggregator.Subscribe(handler, onError);
        }

        public void Subscribe<T>(Action<T> handler, Action<T, Exception> onError = null) where T : class
        {
            _aggregator.Subscribe(handler, onError);
        }

        public void Unsubscribe<T>() where T : class
        {
            _aggregator.Unsubscribe<T>();
        }

        public bool Handle(Type subscriptionType, object message)
        {
            return _aggregator.Handle(subscriptionType, message);
        }

        public bool Publish(object message)
        {
            return _publisher.Publish(message);
        }
    }
}

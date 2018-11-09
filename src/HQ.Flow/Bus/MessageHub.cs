// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Bus
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

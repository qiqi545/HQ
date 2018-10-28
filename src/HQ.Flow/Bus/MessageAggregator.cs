// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HQ.Flow.Bus
{
	public sealed class MessageAggregator : IMessageAggregator
	{
		private static readonly Action<object, Exception> NoopErrorHandler = (m, e) => { };
		private readonly Dictionary<Type, List<int>> _handlers = new Dictionary<Type, List<int>>();

		private readonly Dictionary<Type, HashSet<HandlerDelegate>> _subscriptions =
			new Dictionary<Type, HashSet<HandlerDelegate>>();

		public void Subscribe(object handler, Action<object, Exception> onError = null)
		{
			var type = handler.GetType();
			var interfaces = type.GetInterfaces();
			var consumers = interfaces.Where(i =>
				i.IsGenericType && typeof(IMessageHandler<>).IsAssignableFrom(i.GetGenericTypeDefinition())).ToList();

			if (consumers.Count == 0)
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Action<>))
				{
					const BindingFlags binding = BindingFlags.Instance | BindingFlags.NonPublic;
					var subscribeAction = typeof(MessageAggregator).GetMethod(nameof(SubscribeAction), binding);

					var handlerType = type.GetGenericArguments()[0];
					subscribeAction?.MakeGenericMethod(handlerType)
						.Invoke(this, new[] {handler, onError ?? NoopErrorHandler});
				}

			{
				const BindingFlags binding = BindingFlags.Instance | BindingFlags.NonPublic;
				var subscribeTyped = typeof(MessageAggregator).GetMethod(nameof(SubscribeTyped), binding);
				Debug.Assert(subscribeTyped != null);

				foreach (var consumer in consumers)
				{
					var handlerType = consumer.GetGenericArguments()[0];
					subscribeTyped?.MakeGenericMethod(handlerType)
						.Invoke(this, new[] {handler, onError ?? NoopErrorHandler});
				}
			}
		}

		public void Subscribe<T>(Action<T> handler, Action<T, Exception> onError = null) where T : class
		{
			SubscribeAction(handler, onError);
		}

		public void Subscribe<T>(IMessageHandler<T> handler, Action<T, Exception> onError = null) where T : class
		{
			SubscribeTyped(handler, (o, exception) => { onError?.Invoke(o as T, exception); });
		}

		public void Unsubscribe<T>() where T : class
		{
			var subscriptionType = typeof(T);

			//if (_unsubscriptions.TryGetValue(type, out var cancel))
			//	cancel.Cancel();

			if (_subscriptions.TryGetValue(subscriptionType, out _))
			{
				_subscriptions.Remove(subscriptionType);
				_handlers.Remove(subscriptionType);
			}
		}

		public bool Handle(Type subscriptionType, object message)
		{
			if (!_subscriptions.TryGetValue(subscriptionType, out var subscription))
				return true;

			var result = true;
			foreach (var handler in subscription)
			{
				if (handler == null)
					continue;

				var handled = handler(message);
				result &= handled;
			}

			return result;
		}

		private void SubscribeAction<T>(Action<T> handler, Action<T, Exception> onError) where T : class
		{
			Subscribe(new ActionMessageHandler<T>(handler), (o, exception) => { onError?.Invoke(o, exception); });
		}

		private void SubscribeTyped<T>(IMessageHandler<T> handler, Action<object, Exception> onError)
		{
			var subscriptionType = typeof(T);

			bool TryHandleMessage(object message)
			{
				try
				{
					return handler.Handle((T) message);
				}
				catch (Exception ex)
				{
					onError?.Invoke(message, ex);
					return false;
				}
			}

			var handle = RuntimeHelpers.GetHashCode(handler);
			if (!_subscriptions.TryGetValue(subscriptionType, out var list))
				_subscriptions.Add(subscriptionType,
					list = new HashSet<HandlerDelegate>(ReferenceEqualityComparer<HandlerDelegate>.Instance));

			if (!_handlers.TryGetValue(subscriptionType, out var handlers))
				_handlers.Add(subscriptionType, handlers = new List<int>());

			if (handlers.Contains(handle))
				throw new DuplicateSubscriptionException();

			handlers.Add(handle);

			list.Add(TryHandleMessage);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposing || _subscriptions == null || _subscriptions.Count == 0)
				return;

			foreach (var subscription in _subscriptions)
			{
				if (subscription.Value == null)
					continue;

				if (_subscriptions.Remove(subscription.Key))
				{
					//if (_unsubscriptions.TryGetValue(subscription.Key, out var cancel))
					//	cancel.Cancel();
				}
			}
		}

		private delegate bool HandlerDelegate(object message);

		internal class ActionMessageHandler<T> : IMessageHandler<T>
		{
			private readonly Action<T> _handler;

			public ActionMessageHandler(Action<T> handler)
			{
				_handler = handler;
			}

			public bool Handle(T message)
			{
				_handler?.Invoke(message);
				return true;
			}
		}
	}
}
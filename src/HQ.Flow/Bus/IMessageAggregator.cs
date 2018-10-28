// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Bus
{
	public interface IMessageAggregator
	{
		void Subscribe(object handler, Action<object, Exception> onError = null);
		void Subscribe<T>(IMessageHandler<T> handler, Action<T, Exception> onError = null) where T : class;
		void Subscribe<T>(Action<T> handler, Action<T, Exception> onError = null) where T : class;
		void Unsubscribe<T>() where T : class;
		bool Handle(Type subscriptionType, object message);
	}
}
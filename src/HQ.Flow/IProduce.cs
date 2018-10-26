// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using HQ.Flow.Consumers;

namespace HQ.Flow
{
	/// <summary>
	///     A producer of events that intends to send those events to an attached <see cref="IConsume{T}" />
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IProduce<out T>
	{
		void Attach(IConsume<T> consumer);
	}

	public static class IProducerExtensions
	{
		public static void Attach<T>(this IProduce<T> producer, Action<T> consumer)
		{
			producer.Attach(new ActionConsumer<T>(consumer));
		}
	}
}
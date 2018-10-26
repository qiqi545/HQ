// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading.Tasks;

namespace HQ.Flow.Consumers
{
	/// <summary>
	///     A consumer that executes a delegate against any received events.
	///     It can also optionally forward to another consumer after invoking the delegate action.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ActionConsumer<T> : IConsume<T>
	{
		private readonly Func<T, Task<bool>> _delegate;

		public ActionConsumer(Action<T> @delegate, IConsume<T> forwardTo)
		{
			_delegate = async @event =>
			{
				try
				{
					@delegate(@event);

					return await forwardTo.HandleAsync(@event);
				}
				catch (Exception)
				{
					return false;
				}
			};
		}

		public ActionConsumer(Action<T> @delegate)
		{
			_delegate = @event =>
			{
				try
				{
					@delegate(@event);
					return Task.FromResult(true);
				}
				catch (Exception)
				{
					return Task.FromResult(false);
				}
			};
		}

		public ActionConsumer(Func<T, Task<bool>> @delegate)
		{
			_delegate = @delegate;
		}

		public Task<bool> HandleAsync(T message)
		{
			return _delegate(message);
		}
	}
}
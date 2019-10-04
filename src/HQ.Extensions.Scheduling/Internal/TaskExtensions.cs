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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Extensions.Scheduling.Internal
{
	/// <summary>
	///     See: http://blogs.msdn.com/b/pfxteam/archive/2011/10/24/10229468.aspx
	/// </summary>
	internal static class TaskExtensions
	{
		private static readonly IDictionary<TaskScheduler, TaskFactory> TaskFactories =
			new ConcurrentDictionary<TaskScheduler, TaskFactory>();

		public static Task Run(this TaskScheduler scheduler, Action action, CancellationToken cancellationToken)
		{
			return WithTaskFactory(scheduler).StartNew(action, cancellationToken);
		}

		public static Task Run<T>(this TaskScheduler scheduler, Func<T> func, CancellationToken cancellationToken)
		{
			return WithTaskFactory(scheduler).StartNew(func, cancellationToken);
		}

		public static Task Run(this TaskScheduler scheduler, Func<Task> func, CancellationToken cancellationToken)
		{
			return WithTaskFactory(scheduler).StartNew(func, cancellationToken).Unwrap();
		}

		public static Task Run<T>(this TaskScheduler scheduler, Func<Task<T>> func, CancellationToken cancellationToken)
		{
			return WithTaskFactory(scheduler).StartNew(func, cancellationToken).Unwrap();
		}

		public static TaskFactory WithTaskFactory(this TaskScheduler scheduler)
		{
			if (!TaskFactories.TryGetValue(scheduler, out var tf))
			{
				TaskFactories.Add(scheduler,
					tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.DenyChildAttach,
						TaskContinuationOptions.None, scheduler));
			}

			return tf;
		}
	}
}
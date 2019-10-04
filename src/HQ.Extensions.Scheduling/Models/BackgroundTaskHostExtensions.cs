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
using System.Threading.Tasks;
using HQ.Extensions.Scheduling.Configuration;
using Newtonsoft.Json;

namespace HQ.Extensions.Scheduling.Models
{
	public static class BackgroundTaskHostExtensions
	{
		/// <summary>
		///     Schedules a new task for delayed execution for the given host.
		///     If the user does NOT provide a RunAt during options, but an expression IS provided, the next occurrence of the
		///     expression, relative to now, will be selected as the start time.
		///     Otherwise, the task will be scheduled for now.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="type"></param>
		/// <param name="options">
		///     Allows configuring task-specific features. Note that this is NOT invoked at invocation time
		///     lazily, but at scheduling time (i.e. immediately).
		/// </param>
		/// <returns>
		///     Whether the scheduled operation was successful; if `true`, it was either scheduled or ran successfully,
		///     depending on configuration. If `false`, it either failed to schedule or failed during execution, depending on
		///     configuration.
		/// </returns>
		public static Task<(bool, BackgroundTask)> TryScheduleTaskAsync(this BackgroundTaskHost host, Type type,
			object userData = null, Action<BackgroundTask> options = null)
		{
			return host.QueueForExecutionAsync(type, userData, options);
		}

		private static async Task<(bool, BackgroundTask)> QueueForExecutionAsync(this BackgroundTaskHost host,
			Type type, object userData, Action<BackgroundTask> options)
		{
			var task = NewTask(host.Options, host.Serializer, type, userData);

			options?.Invoke(task); // <-- at this stage, task should have a RunAt set by the user or it will be default

			if (!string.IsNullOrWhiteSpace(task.Expression) && !task.HasValidExpression)
			{
				throw new ArgumentException("The provided CRON expression is invalid.");
			}

			// Handle when no start time is provided up front
			if (task.RunAt == default)
			{
				task.RunAt = host.GetCurrentTime().UtcDateTime;

				if (task.NextOccurrence.HasValue)
				{
					task.RunAt = task.NextOccurrence.Value;
				}
			}

			// Set the "Start" property only once, equal to the very first RunAt 
			task.Start = task.RunAt;

			if (!host.Options.DelayTasks)
			{
				return (await host.AttemptTaskAsync(task, false), task);
			}

			if (host.Store != null)
			{
				await host.Store.SaveAsync(task);
			}

			return (true, task);
		}

		private static BackgroundTask NewTask(BackgroundTaskOptions options, IBackgroundTaskSerializer serializer,
			Type type, object userData)
		{
			var handlerInfo = new HandlerInfo(type.Namespace, type.Name);
			var task = new BackgroundTask
			{
				Handler = serializer?.Serialize(handlerInfo),
				Data = userData == null ? null : JsonConvert.SerializeObject(userData)
			};
			options.ProvisionTask(task);
			return task;
		}
	}
}
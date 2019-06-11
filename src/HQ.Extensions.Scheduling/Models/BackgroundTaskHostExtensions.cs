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
using HQ.Extensions.Scheduling.Configuration;
using TypeKitchen;

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
        /// <typeparam name="T">
        ///     The type of the task. The task is created at trigger time. The trigger type must have a
        ///     default constructor.
        /// </typeparam>
        /// <param name="host"></param>
        /// <param name="options">
        ///     Allows configuring task-specific features. Note that this is NOT invoked at invocation time
        ///     lazily, but at scheduling time (i.e. immediately).
        /// </param>
        /// <param name="configure">
        ///     Allows setting parameters on the scheduled task. Note that this is NOT invoked at invocation
        ///     time lazily, but at scheduling time (i.e. immediately).
        /// </param>
        /// <returns>
        ///     Whether the scheduled operation was successful; if `true`, it was either scheduled or ran successfully,
        ///     depending on configuration. If `false`, it either failed to schedule or failed during execution, depending on
        ///     configuration.
        /// </returns>
        public static bool ScheduleAsync<T>(this BackgroundTaskHost host, Action<BackgroundTask> options = null,
            Action<T> configure = null) where T : class, new()
        {
            if (configure == null)
            {
                return QueueForExecution<T>(host, options);
            }

            var instance = (T) Instancing.CreateInstance(typeof(T));
            configure(instance);

            return QueueForExecution<T>(host, options, instance);
        }

        private static bool QueueForExecution<T>(this BackgroundTaskHost producer, Action<BackgroundTask> options,
            object instance = null)
        {
            var task = NewTask<T>(producer.Settings, instance);

            options?.Invoke(task); // <-- at this stage, task should have a RunAt set by the user or it will be default

            if (!string.IsNullOrWhiteSpace(task.Expression) && !task.HasValidExpression)
            {
                throw new ArgumentException(
                    "The provided CRON expression is invalid. Have you tried the CronTemplates?");
            }

            // Handle when no start time is provided up front
            if (task.RunAt == default)
            {
                task.RunAt = DateTimeOffset.UtcNow;

                if (task.NextOccurrence.HasValue)
                {
                    task.RunAt = task.NextOccurrence.Value;
                }
            }

            // Set the "Start" property only once, equal to the very first RunAt 
            task.Start = task.RunAt;

            if (!producer.Settings.DelayTasks)
            {
                return producer.AttemptTask(task, false);
            }

            producer.Settings.Store?.Save(task);
            return true;
        }

        private static BackgroundTask NewTask<T>(BackgroundTaskSettings settings, object instance = null)
        {
            var type = typeof(T);

            var handlerInfo = new HandlerInfo(type.Namespace, type.Name);
            if (instance != null)
            {
                handlerInfo.Instance = settings.HandlerSerializer.Serialize(instance);
            }

            var scheduledTask = new BackgroundTask {Handler = settings.HandlerSerializer.Serialize(handlerInfo)};
            settings.ProvisionTask(scheduledTask);
            return scheduledTask;
        }
    }
}

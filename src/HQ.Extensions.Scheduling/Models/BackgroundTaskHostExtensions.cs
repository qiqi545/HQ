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
        /// <param name="task"></param>
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
        public static bool TryScheduleTask<T>(this BackgroundTaskHost host, out BackgroundTask task, Action<BackgroundTask> options = null, Action<T> configure = null) where T : class, new()
        {
            if (configure == null)
                return host.QueueForExecution<T>(out task, options);

            var instance = Instancing.CreateInstance<T>();
            configure(instance);

            return host.QueueForExecution<T>(out task, options, instance);
        }

        public static bool TryScheduleTask(this BackgroundTaskHost host, Type type, out BackgroundTask task, Action<BackgroundTask> options = null, Action<object> configure = null)
        {
            if (configure == null)
                return host.QueueForExecution(type, out task, options);

            var instance = Instancing.CreateInstance(type);
            configure(instance);

            return host.QueueForExecution(type, out task, options, instance);
        }

        private static bool QueueForExecution<T>(this BackgroundTaskHost host, out BackgroundTask task, Action<BackgroundTask> options, object instance = null)
        {
            return host.QueueForExecution(typeof(T), out task, options, instance);
        }

        private static bool QueueForExecution(this BackgroundTaskHost host, Type type, out BackgroundTask task, Action<BackgroundTask> options, object instance = null)
        {
            task = NewTask(host.Options, host.Serializer, type, instance);

            options?.Invoke(task); // <-- at this stage, task should have a RunAt set by the user or it will be default

            if (!string.IsNullOrWhiteSpace(task.Expression) && !task.HasValidExpression)
            {
                throw new ArgumentException("The provided CRON expression is invalid. Have you tried CronTemplates?");
            }

            // Handle when no start time is provided up front
            if (task.RunAt == default)
            {
                task.RunAt = host.GetCurrentTime();

                if (task.NextOccurrence.HasValue)
                {
                    task.RunAt = task.NextOccurrence.Value;
                }
            }

            // Set the "Start" property only once, equal to the very first RunAt 
            task.Start = task.RunAt;

            if (!host.Options.DelayTasks)
            {
                return host.AttemptTask(task, false);
            }

            host.Store?.Save(task);
            return true;
        }

        private static BackgroundTask NewTask(BackgroundTaskOptions options, IBackgroundTaskSerializer serializer, Type type, object instance = null)
        {
            var handlerInfo = new HandlerInfo(type.Namespace, type.Name);
            if (instance != null)
            {
                handlerInfo.Instance = serializer?.Serialize(instance);
            }

            var task = new BackgroundTask { Handler = serializer?.Serialize(handlerInfo) };
            options.ProvisionTask(task);
            return task;
        }
    }
}

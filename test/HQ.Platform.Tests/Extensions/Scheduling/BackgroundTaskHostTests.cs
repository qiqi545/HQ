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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Platform.Tests.Extensions.Scheduling.Handlers;
using HQ.Test.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Tests.Extensions.Scheduling
{
	public abstract class BackgroundTaskHostTests : ServiceUnderTest
	{
		protected readonly IBackgroundTaskStore Store;

		protected BackgroundTaskHostTests(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			Store = ServiceProvider.GetRequiredService(typeof(IBackgroundTaskStore)) as IBackgroundTaskStore;
		}
		
		[Test]
		public async Task Queues_for_delayed_execution()
		{
			var host = CreateBackgroundTaskHost(o =>
            {
                o.DelayTasks = true;
                o.Concurrency = 1;
                o.SleepIntervalSeconds = 1;
            });
            await host.TryScheduleTaskAsync(typeof(StaticCountingHandler), null, o => { o.RunAt =
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1); });

            Assert.True(StaticCountingHandler.Count == 0, "handler should not have queued immediately since tasks are delayed");

			host.Start(); // <-- starts background thread to poll for tasks
            
            await Task.Delay(2000); // <-- enough time for the next occurrence
            Assert.True(StaticCountingHandler.Count > 0, "handler should have executed since we scheduled it in the future");
			Assert.True(StaticCountingHandler.Count == 1, "handler should have only executed once since it does not repeat");
		}

        [Test]
		public void Queues_for_delayed_execution_and_continuous_repeating_task()
		{
            var host = CreateBackgroundTaskHost(o =>
            {
                o.DelayTasks = true;
                o.Concurrency = 1;
                o.SleepIntervalSeconds = 1;
            });
            host.TryScheduleTaskAsync(typeof(StaticCountingHandler), null, o =>
            {
                o.RunAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1);
                o.RepeatIndefinitely(CronTemplates.Secondly(1));
            });
            host.Start(); // <-- starts background thread to poll for tasks

            Assert.True(StaticCountingHandler.Count == 0, "handler should not have queued immediately since tasks are delayed");
            Thread.Sleep(TimeSpan.FromSeconds(2)); // <-- enough time for the next occurrence
            Assert.True(StaticCountingHandler.Count > 0, "handler should have executed since we scheduled it in the future");
            Thread.Sleep(TimeSpan.FromSeconds(2)); // <-- enough time for the next occurrence
            Assert.True(StaticCountingHandler.Count >= 2);
        }

		[Test]
		public void Queues_for_immediate_execution()
		{
            var host = CreateBackgroundTaskHost(o => { o.DelayTasks = false; });
            host.TryScheduleTaskAsync(typeof(StaticCountingHandler));

            Thread.Sleep(TimeSpan.FromMilliseconds(100)); // <-- enough time for the occurrence

			Assert.True(StaticCountingHandler.Count == 1,
				"handler should have queued immediately since tasks are not delayed");
		}

		[Test]
		public void Starts_and_stops()
		{
			using var host = CreateBackgroundTaskHost(o => { });
			host.Start();
			host.Stop();
		}

		[Test]
		public async Task Can_cleanup_hanging_tasks()
		{
			using var host = CreateBackgroundTaskHost(o =>
			{
				o.DelayTasks = true;
				o.MaximumAttempts = 1;
				o.MaximumRuntimeSeconds = 1;
				o.CleanupIntervalSeconds = 1000;
				o.SleepIntervalSeconds = 1;
			});
			
			host.Start();
			{
				await host.TryScheduleTaskAsync(typeof(TerminalTaskHandler));

				var all = (await Store.GetAllAsync()).ToList();
				Assert.Equal(1, all.Count, "Queue task should exist");

				await Task.Delay(TimeSpan.FromSeconds(2)); // <-- enough time to have started the terminal task

				all = (await Store.GetAllAsync()).ToList();
				Assert.Equal(1, all.Count, "Queue task should still exist, since it is terminal");
				
				var task = all.First();
				Assert.True(task.LockedAt.HasValue, "Queue task should be locked");
				Assert.True(task.MaximumRuntime.HasValue, "Queue task should be have a maximum runtime set.");
				Assert.True(task.RunningOvertime, "Queue task should be running overtime");

				var hanging = (await Store.GetHangingTasksAsync()).ToList();
				Assert.Equal(1, hanging.Count, "Hanging task is not considered hanging by the task store");

				await host.CleanUpHangingTasksAsync();
			}
			
			host.Stop();
		}

		private BackgroundTaskHost CreateBackgroundTaskHost(Action<BackgroundTaskOptions> configureOptions)
        {
            var services = new ServiceCollection();
            services.AddBackgroundTasks(configureOptions);
            var serviceProvider = services.BuildServiceProvider();
			var timestamps = serviceProvider.GetRequiredService<IServerTimestampService>();
			var serializer = new JsonBackgroundTaskSerializer();
			var typeResolver = new ReflectionTypeResolver();
			var options = serviceProvider.GetRequiredService<IOptionsMonitor<BackgroundTaskOptions>>();
			var host = serviceProvider.GetService<ISafeLogger<BackgroundTaskHost>>();

			var scheduler = new BackgroundTaskHost(serviceProvider, timestamps, Store, serializer, typeResolver, options, host);
            return scheduler;
        }
	}
}

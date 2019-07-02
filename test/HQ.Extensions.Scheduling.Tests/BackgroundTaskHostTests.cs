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
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Extensions.Scheduling.Tests
{
	public class BackgroundTaskHostTests : UnitUnderTest
	{
		[Test]
		public async Task Queues_for_delayed_execution()
		{
			var host = CreateBackgroundTaskHost(o =>
            {
                o.DelayTasks = true;
                o.Concurrency = 1;
                o.SleepIntervalSeconds = 1;
            });
            await host.TryScheduleTaskAsync(typeof(StaticCountingHandler), o => { o.RunAt =
                DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1); });

            host.Start(); // <-- starts background thread to poll for tasks
            
            Assert.True(StaticCountingHandler.Count == 0, "handler should not have queued immediately since tasks are delayed");
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
            host.TryScheduleTaskAsync(typeof(StaticCountingHandler), o =>
            {
                o.RunAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1);
                o.RepeatIndefinitely(CronTemplates.Secondly(1));
            });
            host.Start(); // <-- starts background thread to poll for tasks

            Assert.True(StaticCountingHandler.Count == 0, "handler should not have queued immediately since tasks are delayed");
            Thread.Sleep(TimeSpan.FromSeconds(2)); // <-- enough time for the next occurrence
            Assert.True(StaticCountingHandler.Count > 0, "handler should have executed since we scheduled it in the future");
            Thread.Sleep(TimeSpan.FromSeconds(2)); // <-- enough time for the next occurrence
            Assert.Equal(2, StaticCountingHandler.Count);
        }

		[Test]
		public void Queues_for_immediate_execution()
		{
            var host = CreateBackgroundTaskHost(o => { o.DelayTasks = false; });
            host.TryScheduleTaskAsync(typeof(StaticCountingHandler));

			Assert.True(StaticCountingHandler.Count == 1,
				"handler should have queued immediately since tasks are not delayed");
		}

		[Test]
		public void Starts_and_stops()
		{
            var host = CreateBackgroundTaskHost(o => { });
            host.Start();
			host.Stop();
		}

        private static BackgroundTaskHost CreateBackgroundTaskHost(Action<BackgroundTaskOptions> configureOptions)
        {
            var services = new ServiceCollection();
            services.AddBackgroundTasks(configureOptions);
            var serviceProvider = services.BuildServiceProvider();
            var timestamps = new LocalServerTimestampService();
            var scheduler = new BackgroundTaskHost(serviceProvider, timestamps,
                new InMemoryBackgroundTaskStore(timestamps), new JsonBackgroundTaskSerializer(),
                new ReflectionTypeResolver(),
                serviceProvider.GetRequiredService<IOptionsMonitor<BackgroundTaskOptions>>(),
                serviceProvider.GetService<ISafeLogger<BackgroundTaskHost>>());
            return scheduler;
        }
	}
}

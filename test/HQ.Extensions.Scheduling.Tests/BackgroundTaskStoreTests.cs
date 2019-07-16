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
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Test.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Scheduling.Tests
{
    public abstract class BackgroundTaskStoreTests : ServiceUnderTest
    {
        protected readonly IBackgroundTaskStore Store;
        private readonly IServerTimestampService _timestamps;

        protected BackgroundTaskStoreTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Store = ServiceProvider.GetRequiredService(typeof(IBackgroundTaskStore)) as IBackgroundTaskStore;
            _timestamps = ServiceProvider.GetRequiredService(typeof(IServerTimestampService)) as IServerTimestampService;
        }

        [Test, Isolated]
        public async Task Adding_tags_synchronizes_with_store()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            await Store.SaveAsync(create);

            var created = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            created.Tags.Add("b");
            await Store.SaveAsync(created);

            created.Tags.Add("c");
            await Store.SaveAsync(created);

            var all = await Store.GetAllAsync();
            Assert.Equal(1, all.Count());
            Assert.Equal(3, all.Single().Tags.Count);
        }

        [Test, Isolated]
        public async Task Can_delete_tasks_with_tags()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");

            await Store.SaveAsync(create);

            var created = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            await Store.DeleteAsync(create);

            var deleted = await Store.GetByIdAsync(create.Id);
            Assert.Null(deleted);
        }

        [Test, Isolated]
        public async Task Can_save_multiple_tasks_with_tags()
        {
            var first = CreateNewTask();
            first.Tags.Add("one");
            await Store.SaveAsync(first);

            var second = CreateNewTask();
            second.Tags.Add("two");
            await Store.SaveAsync(second);

            var tasks = await Store.GetByAllTagsAsync("one");
            Assert.True(tasks.Count() == 1);
            Assert.True(tasks.Single().Tags.Count == 1);

            await Store.DeleteAsync(second);
        }

        [Test, Isolated]
        public async Task Can_search_for_all_tags()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");
            await Store.SaveAsync(create);

            var created = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            // GetByAllTagsAsync (miss):
            var all = await Store.GetByAllTagsAsync("a", "b", "c", "d");
            Assert.Equal(0, all.Count(), "Result returned that doesn't contain all tags");

            // GetByAnyTagsAsync (hit):
            all = await Store.GetByAllTagsAsync("a", "b", "c");
            Assert.Equal(1, all.Count());
            Assert.Equal(3, all.Single().Tags.Count);
        }

        [Test, Isolated]
        public async Task Can_search_for_any_tags()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");
            await Store.SaveAsync(create);

            var created = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            // GetByAllTagsAsync (miss):
            var all = await Store.GetByAnyTagsAsync("e");
            Assert.Equal(0, all.Count());

            // GetByAnyTagsAsync (hit):
            all = await Store.GetByAnyTagsAsync("e", "a");
            Assert.Equal(1, all.Count());
            Assert.Equal(3, all.Single().Tags.Count);
        }

        [Test, Isolated]
        public async Task Inserts_new_task()
        {
            var create = CreateNewTask();

            Assert.True(create.Id == 0);
            await Store.SaveAsync(create);
            Assert.False(create.Id == 0);

            var created = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");
            Assert.Equal(create.Id, created.Id);
        }

        [Test, Isolated]
        public async Task Locked_tasks_are_not_visible_to_future_fetches()
        {
            var created = CreateNewTask();

            await Store.SaveAsync(created);

            var locked = await Store.LockNextAvailableAsync(int.MaxValue);
            Assert.False(!locked.Any(), "did not retrieve at least one unlocked task");

            locked = await Store.LockNextAvailableAsync(int.MaxValue);
            Assert.True(!locked.Any(), "there was at least one unlocked task after locking all of them");
        }

        [Test, Isolated]
        public async Task Removing_tags_synchronizes_with_store()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");
            await Store.SaveAsync(create);

            var created = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            create.Tags.Remove("a");
            await Store.SaveAsync(create);

            var all = await Store.GetAllAsync();
            Assert.Equal(1, all.Count());
            Assert.Equal(2, all.First().Tags.Count);

            var byId = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(byId);
            Assert.Equal(2, byId.Tags.Count);

            create.Tags.Clear();
            await Store.SaveAsync(create);

            byId = await Store.GetByIdAsync(create.Id);
            Assert.NotNull(byId);
            Assert.True(byId.Tags.Count == 0);
        }

        [Test, Isolated]
        public async Task Tags_are_saved_with_tasks()
        {
            var created = CreateNewTask();
            created.Tags.Add("a");
            created.Tags.Add("b");
            created.Tags.Add("c");
            await Store.SaveAsync(created);

            var all = await Store.GetAllAsync();
            Assert.Equal(1, all.Count());
            Assert.Equal(3, all.Single().Tags.Count);

            var byId = await Store.GetByIdAsync(created.Id);
            Assert.NotNull(byId);
            Assert.Equal(3, byId.Tags.Count);

            var locked = await Store.LockNextAvailableAsync(1);
            Assert.Equal(1, locked.Count());
            Assert.Equal(3, locked.Single().Tags.Count);
        }

        private BackgroundTask CreateNewTask()
        {
            var task = new BackgroundTask();
            var options = new BackgroundTaskOptions();

            // these values are required and must be set by implementation
            task.Handler = "{}";
            task.MaximumAttempts = options.MaximumAttempts;
            task.MaximumRuntime = TimeSpan.FromSeconds(options.MaximumRuntimeSeconds);
            task.DeleteOnError = options.DeleteOnError;
            task.DeleteOnFailure = options.DeleteOnFailure;
            task.DeleteOnSuccess = options.DeleteOnSuccess;
            task.RunAt = _timestamps.GetCurrentTime().LocalDateTime;

            return task;
        }
    }
}

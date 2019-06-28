using System;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Data;
using HQ.Test.Sdk.Xunit;

namespace HQ.Extensions.Scheduling.Tests
{
    public abstract class BackgroundTaskStoreTests : ServiceUnderTest
    {
        protected readonly IBackgroundTaskStore Store;

        protected BackgroundTaskStoreTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Store = ServiceProvider.GetService(typeof(IBackgroundTaskStore)) as IBackgroundTaskStore;
        }

        [Test, Isolated]
        public void Adding_tags_synchronizes_with_store()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            Store.Save(create);

            var created = Store.GetById(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            created.Tags.Add("b");
            Store.Save(created);

            created.Tags.Add("c");
            Store.Save(created);

            var all = Store.GetAll();
            Assert.Equal(1, all.Count);
            Assert.Equal(3, all[0].Tags.Count);
        }

        [Test, Isolated]
        public void Can_delete_tasks_with_tags()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");

            Store.Save(create);

            var created = Store.GetById(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            Store.Delete(create);

            var deleted = Store.GetById(create.Id);
            Assert.Null(deleted);
        }

        [Test, Isolated]
        public void Can_save_multiple_tasks_with_tags()
        {
            var first = CreateNewTask();
            first.Tags.Add("one");
            Store.Save(first);

            var second = CreateNewTask();
            second.Tags.Add("two");
            Store.Save(second);

            var tasks = Store.GetByAllTags("one");
            Assert.True(tasks.Count == 1);
            Assert.True(tasks[0].Tags.Count == 1);

            Store.Delete(second);
        }

        [Test, Isolated]
        public void Can_search_for_all_tags()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");
            Store.Save(create);

            var created = Store.GetById(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            // GetByAllTags (miss):
            var all = Store.GetByAllTags("a", "b", "c", "d");
            Assert.Equal(0, all.Count);

            // GetByAnyTags (hit):
            all = Store.GetByAllTags("a", "b", "c");
            Assert.Equal(1, all.Count);
            Assert.Equal(3, all[0].Tags.Count);
        }

        [Test, Isolated]
        public void Can_search_for_any_tags()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");
            Store.Save(create);

            var created = Store.GetById(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            // GetByAllTags (miss):
            var all = Store.GetByAnyTags("e");
            Assert.Equal(0, all.Count);

            // GetByAnyTags (hit):
            all = Store.GetByAnyTags("e", "a");
            Assert.Equal(1, all.Count);
            Assert.Equal(3, all[0].Tags.Count);
        }

        [Test, Isolated]
        public void Inserts_new_task()
        {
            var create = CreateNewTask();

            Assert.True(create.Id == 0);
            Store.Save(create);
            Assert.False(create.Id == 0);

            var created = Store.GetById(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");
            Assert.Equal(create.Id, created.Id);
        }

        [Test, Isolated]
        public void Locked_tasks_are_not_visible_to_future_fetches()
        {
            var created = CreateNewTask();

            Store.Save(created);

            var locked = Store.LockNextAvailable(int.MaxValue);
            Assert.False(locked.Count == 0, "did not retrieve at least one unlocked task");

            locked = Store.LockNextAvailable(int.MaxValue);
            Assert.True(locked.Count == 0, "there was at least one unlocked task after locking all of them");
        }

        [Test, Isolated]
        public void Removing_tags_synchronizes_with_store()
        {
            var create = CreateNewTask();
            create.Tags.Add("a");
            create.Tags.Add("b");
            create.Tags.Add("c");
            Store.Save(create);

            var created = Store.GetById(create.Id);
            Assert.NotNull(created, $"Created task with ID '{create.Id}' is not visible");

            create.Tags.Remove("a");
            Store.Save(create);

            // GetAll:
            var all = Store.GetAll();
            Assert.Equal(1, all.Count);
            Assert.Equal(2, all[0].Tags.Count);

            // GetById:
            var byId = Store.GetById(create.Id);
            Assert.NotNull(byId);
            Assert.Equal(2, byId.Tags.Count);

            create.Tags.Clear();
            Store.Save(create);

            // GetById:
            byId = Store.GetById(create.Id);
            Assert.NotNull(byId);
            Assert.True(byId.Tags.Count == 0);
        }

        [Test, Isolated]
        public void Tags_are_saved_with_tasks()
        {
            var created = CreateNewTask();
            created.Tags.Add("a");
            created.Tags.Add("b");
            created.Tags.Add("c");
            Store.Save(created);

            // GetAll:
            var all = Store.GetAll();
            Assert.Equal(1, all.Count);
            Assert.Equal(3, all[0].Tags.Count);

            // GetById:
            var byId = Store.GetById(created.Id);
            Assert.NotNull(byId);
            Assert.Equal(3, byId.Tags.Count);

            // LockNextAvailable:
            var locked = Store.LockNextAvailable(1);
            Assert.Equal(1, locked.Count);
            Assert.Equal(3, locked[0].Tags.Count);
        }

        private static BackgroundTask CreateNewTask()
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

            return task;
        }
    }
}

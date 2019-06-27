using System;
using HQ.Extensions.Scheduling.Models;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Xunit;

namespace HQ.Extensions.Scheduling.Tests
{
    public abstract class BackgroundTaskStoreTests : ServiceUnderTest
    {
        private readonly IBackgroundTaskStore _store;

        protected BackgroundTaskStoreTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _store = ServiceProvider.GetService(typeof(IBackgroundTaskStore)) as IBackgroundTaskStore;
        }

        [Test]
        public void Can_create_background_task()
        {
            var task = new BackgroundTask
            {
                Start = DateTimeOffset.UtcNow,
                Handler = "NoHandler",
                MaximumRuntime = TimeSpan.FromSeconds(10),
                MaximumAttempts = 10,
                DeleteOnError = true,
                DeleteOnFailure = false,
                DeleteOnSuccess = true
            };
            _store.Save(task);
            Assert.True(task.Id != 0);
            Assert.True(task.CreatedAt != default);
        }

        [Test]
        public void Can_delete_background_task()
        {
            var task = new BackgroundTask
            {
                Start = DateTimeOffset.UtcNow,
                Handler = "NoHandler",
                MaximumRuntime = TimeSpan.FromSeconds(10),
                MaximumAttempts = 10,
                DeleteOnError = true,
                DeleteOnFailure = false,
                DeleteOnSuccess = true
            };
            Assert.True(_store.Save(task), "Did not save");
            Assert.True(_store.Delete(task), "Did not delete");
        }
    }
}

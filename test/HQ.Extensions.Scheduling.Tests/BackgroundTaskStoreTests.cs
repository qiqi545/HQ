using System;
using System.Threading.Tasks;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Xunit;

namespace HQ.Extensions.Scheduling.Tests
{
    public abstract class BackgroundTaskStoreTests : ServiceUnderTest
    {
        protected BackgroundTaskStoreTests(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [Test]
        public async Task Can_create_background_task()
        {
            
        }
    }
}

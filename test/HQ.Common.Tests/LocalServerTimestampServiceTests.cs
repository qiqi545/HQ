using System;
using Xunit;

namespace HQ.Common.Tests
{
    public class LocalServerTimestampServiceTests
    {
        [Fact]
        public void Can_get_local_server_time()
        {
            var service = new LocalServerTimestampService();
            var currentTime = service.GetCurrentTime();
            Assert.NotEqual(default(DateTimeOffset), currentTime);
        }
    }
}

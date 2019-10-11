using System;
using HQ.Extensions.Dates;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Platform.Tests.Extensions.Dates
{
    public class DateTimeZoneTests
    {
        [Test]
        public void BasicTests()
        {
            Assert.Equal(TimeSpan.Zero, DateTimeZone.UtcNow.Offset);
        }
    }
}

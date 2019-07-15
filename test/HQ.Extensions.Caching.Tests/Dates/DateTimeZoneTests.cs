using System;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Extensions.Dates.Tests
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

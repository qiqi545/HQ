using System;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Common.Tests
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

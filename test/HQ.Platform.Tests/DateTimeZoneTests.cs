using System;
using HQ.Common;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Platform.Tests
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

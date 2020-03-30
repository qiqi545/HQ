using System;
using System.IO;
using HQ.Common;
using HQ.Test.Sdk;

namespace HQ.Platform.Tests.Common
{
    public class LocalServerTimestampServiceTests : UnitUnderTest
    {
        [Test]
        public void Can_get_zoned_time()
        {
            var service = new LocalServerTimestampService();
            var currentTime = service.GetCurrentZonedTime();
            Assert.NotEqual(default(DateTimeOffset), currentTime);
        }

        [Test]
        public void Timestamps_do_not_round_trip()
        {
            var service = new LocalServerTimestampService();

            {
                var currentTimestamp = service.GetCurrentTimestamp();
                var deserialized = new DateTimeOffset(new DateTime(currentTimestamp));
                Assert.True(TimeZoneInfo.Local.BaseUtcOffset == TimeSpan.Zero || deserialized.Offset != TimeSpan.Zero);
            }

            {
                var currentTime = service.GetCurrentTime();
                currentTime = currentTime.ToOffset(TimeSpan.FromHours(1)); // non-local offset

                var ms = new MemoryStream();

                var d = currentTime;
                var w = new BinaryWriter(ms);
                w.Write(d.Ticks);

                ms.Position = 0;

                var r = new BinaryReader(ms);
                var recreatedTime = new DateTimeOffset(new DateTime(r.ReadInt64()));

                // We didn't write d.Offset, so we can't recreate the instant in time.
                // This would require 64 bits (d.Ticks) + 16 bits (d.Offset), which does not fit in long.
                Assert.NotEqual(currentTime, recreatedTime);
            }
        }
    }
}

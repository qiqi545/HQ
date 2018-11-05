// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Diagnostics;
using Xunit;

namespace HQ.Cadence.Tests
{
    public class TimeUnitTests
    {
        [Fact]
        public void Can_convert_nanoseconds_to_nanoseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Nanoseconds.ToNanos(expected);
            Trace.WriteLine(actual);
            Assert.Equal(1000L, actual);
        }

        [Fact]
        public void Can_convert_nanoseconds_to_microseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Nanoseconds.ToMicros(expected);
            Trace.WriteLine(actual);
            Assert.Equal(1L, actual);
        }

        [Fact]
        public void Can_convert_nanoseconds_to_milliseconds()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Nanoseconds.ToMillis(expected);
            Trace.WriteLine(actual);
            Assert.Equal(10, actual);
        }

        [Fact]
        public void Can_convert_nanoseconds_to_seconds()
        {
            const long expected = 10000000000;
            var actual = TimeUnit.Nanoseconds.ToSeconds(expected);
            Trace.WriteLine(actual);
            Assert.Equal(10, actual);
        }

        [Fact]
        public void Can_convert_milliseconds_to_nanoseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Milliseconds.ToNanos(expected);
            Trace.WriteLine(actual);
            Assert.Equal(1000000000L, actual);
        }

        [Fact]
        public void Can_convert_milliseconds_to_microseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Milliseconds.ToMicros(expected);
            Trace.WriteLine(actual);
            Assert.Equal(1000000L, actual);
        }

        [Fact]
        public void Can_convert_milliseconds_to_milliseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Milliseconds.ToMillis(expected);
            Trace.WriteLine(actual);
            Assert.Equal(1000L, actual);
        }

        [Fact]
        public void Can_convert_milliseconds_to_seconds()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Milliseconds.ToSeconds(expected);
            Trace.WriteLine(actual);
            Assert.Equal(10000, actual);
        }

        [Fact]
        public void Can_convert_milliseconds_to_minutes()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Milliseconds.ToMinutes(expected);
            Trace.WriteLine(actual);
            Assert.Equal(166, actual);
        }
    }
}

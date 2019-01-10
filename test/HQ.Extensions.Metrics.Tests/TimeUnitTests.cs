#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Diagnostics;
using Xunit;

namespace HQ.Extensions.Metrics.Tests
{
    public class TimeUnitTests
    {
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
        public void Can_convert_milliseconds_to_minutes()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Milliseconds.ToMinutes(expected);
            Trace.WriteLine(actual);
            Assert.Equal(166, actual);
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
        public void Can_convert_milliseconds_to_seconds()
        {
            const long expected = 10000000;
            var actual = TimeUnit.Milliseconds.ToSeconds(expected);
            Trace.WriteLine(actual);
            Assert.Equal(10000, actual);
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
        public void Can_convert_nanoseconds_to_nanoseconds()
        {
            const long expected = 1000;
            var actual = TimeUnit.Nanoseconds.ToNanos(expected);
            Trace.WriteLine(actual);
            Assert.Equal(1000L, actual);
        }

        [Fact]
        public void Can_convert_nanoseconds_to_seconds()
        {
            const long expected = 10000000000;
            var actual = TimeUnit.Nanoseconds.ToSeconds(expected);
            Trace.WriteLine(actual);
            Assert.Equal(10, actual);
        }
    }
}

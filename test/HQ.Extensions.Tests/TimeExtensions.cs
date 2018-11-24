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

using System;

namespace HQ.Extensions.Tests
{
    internal static class TimeExtensions
    {
        public static double Elapsed(this DateTime dateTime)
        {
            return (DateTime.Now.Ticks - dateTime.Ticks).Elapsed();
        }

        public static double Elapsed(this long ticks)
        {
            return (DateTime.Now.Ticks - ticks).Ticks().TotalSeconds;
        }

        public static TimeSpan Ticks(this long value)
        {
            return TimeSpan.FromTicks(value);
        }

        public static TimeSpan Days(this int value)
        {
            return TimeSpan.FromDays(value);
        }

        public static TimeSpan Months(this int value)
        {
            return TimeSpan.FromDays(value * 30);
        }

        public static TimeSpan Weeks(this int value)
        {
            return TimeSpan.FromDays(value * 7);
        }

        public static DateTime FromNow(this TimeSpan value)
        {
            return DateTime.UtcNow.Add(value);
        }

        public static TimeSpan Hours(this int value)
        {
            return TimeSpan.FromHours(value);
        }

        public static TimeSpan Minutes(this int value)
        {
            return TimeSpan.FromMinutes(value);
        }

        public static TimeSpan Seconds(this int value)
        {
            return TimeSpan.FromSeconds(value);
        }

        public static TimeSpan Milliseconds(this int value)
        {
            return TimeSpan.FromMilliseconds(value);
        }

        public static TimeSpan Passed(this DateTime time)
        {
            return DateTime.Now.Subtract(time).Duration();
        }
    }
}

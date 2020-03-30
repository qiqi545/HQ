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
using HQ.Common.Dates;
using HQ.Test.Sdk;

namespace HQ.Platform.Tests.Common.Dates
{
    public class DateSpanTests : UnitUnderTest
    {
        [Test]
        public void Can_get_date_difference_in_days()
        {
            var now = DateTimeOffset.Now;
            var start = now;
            var end = now.AddDays(5);
            var diff = DateSpan.GetDifference(DateInterval.Days, start, end);

            Assert.Equal(5, diff);
        }

        [Test]
        public void Can_get_date_difference_in_days_spanning_one_month()
        {
            var start = new DateTime(2009, 09, 30);
            var end = new DateTime(2009, 10, 01);

            var days = DateSpan.GetDifference(DateInterval.Days, start, end);
            Assert.Equal(1, days);
        }

        [Test]
        public void Can_get_date_difference_in_days_spanning_one_week()
        {
            var start = new DateTime(2009, 09, 30);
            var end = start.AddDays(10);

            var days = DateSpan.GetDifference(DateInterval.Days, start, end);
            var weeks = DateSpan.GetDifference(DateInterval.Weeks, start, end);

            Assert.Equal(10, days);
            Assert.Equal(1, weeks);
        }

        [Test]
        public void Can_get_date_difference_in_days_spanning_two_months()
        {
            var start = new DateTime(2009, 09, 30);
            var end = new DateTime(2009, 11, 04); // 4 days in November, 31 in October

            var days = DateSpan.GetDifference(DateInterval.Days, start, end);
            Assert.Equal(35, days);
        }

        [Test]
        public void Can_handle_composite_spans()
        {
            var start = new DateTime(2009, 9, 30);
            var end = new DateTime(2009, 10, 31);
            var span = new DateSpan(start, end, true /* excludeEndDate */);

            Assert.Equal(1, span.Months);
            Assert.Equal(1, span.Days);

            LogTrace(span.Months.ToString());
            LogTrace(span.Days.ToString());

            var difference = DateSpan.GetDifference(DateInterval.Days, start, end);
            LogTrace(difference.ToString());
        }

        [Test]
        public void Can_get_date_difference_in_seconds()
        {
            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(5);
            var diff = DateSpan.GetDifference(DateInterval.Seconds, start, end);

            Assert.Equal(432000, diff);
        }
    }
}

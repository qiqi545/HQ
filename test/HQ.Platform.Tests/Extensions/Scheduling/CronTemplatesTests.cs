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
using ActiveScheduler.Models;
using HQ.Test.Sdk;
using NCrontab;
using Xunit;

namespace HQ.Platform.Tests.Extensions.Scheduling
{
    public class CronTemplatesTests : UnitUnderTest
    {
        [DataDrivenTest]
        [InlineData(1)]
        [InlineData(5)]
        public void Every_n_seconds(int n)
        {
            var cron = CronTemplates.Secondly(n);
            var schedule = CronTemplates.Parse(cron);
            var diff = CompareTwoCronOccurrences(schedule);
            Assert.Equal(n, diff.Seconds);
        }

        [DataDrivenTest]
        [InlineData(1)]
        [InlineData(5)]
        public void Every_n_minutes(int n)
        {
            var cron = CronTemplates.Minutely(n);
            var schedule = CronTemplates.Parse(cron);
            var diff = CompareTwoCronOccurrences(schedule);
            Assert.Equal(n, diff.Minutes);
        }

        [DataDrivenTest]
        [InlineData(1)]
        public void Every_n_hours(int n)
        {
            var cron = CronTemplates.Hourly(n);
            var schedule = CronTemplates.Parse(cron);
            var diff = CompareTwoCronOccurrences(schedule);
            Assert.Equal(n, diff.Hours);
        }

        [DataDrivenTest]
        [InlineData(1)]
        [InlineData(5)]
        public void Every_n_days(int n)
        {
            var cron = CronTemplates.Daily(n);
            var schedule = CronTemplates.Parse(cron);
            var diff = CompareTwoCronOccurrences(schedule);
            Assert.Equal(n, diff.Days);
        }

        [DataDrivenTest]
        [InlineData(DayOfWeek.Sunday)]
        public void Every_nth_weekday(DayOfWeek n)
        {
            var cron = CronTemplates.Weekly(n);
            var schedule = CronTemplates.Parse(cron);
            var diff = CompareTwoCronOccurrences(schedule);
            Assert.Equal(7, diff.Days);
        }

        [DataDrivenTest]
        [InlineData(DayOfWeek.Monday, DayOfWeek.Tuesday, 1)]
        [InlineData(DayOfWeek.Monday, DayOfWeek.Wednesday, 2)]
        [InlineData(DayOfWeek.Monday, DayOfWeek.Thursday, 3)]
        [InlineData(DayOfWeek.Monday, DayOfWeek.Friday, 4)]
        [InlineData(DayOfWeek.Monday, DayOfWeek.Saturday, 5)]
        [InlineData(DayOfWeek.Monday, DayOfWeek.Sunday, 6)]
        public void Every_nth_and_mth_weekday(DayOfWeek n, DayOfWeek m, int expected)
        {
            var cron = CronTemplates.Weekly(onDays: new[] {n, m});
            var schedule = CronTemplates.Parse(cron);

            // These tests would be temporal if we used 'now', so must start from a known fixed date
            var start = new DateTime(2016, 9, 4);
            var from = schedule.GetNextOccurrence(start); // should always start on 9/5/2016 (Monday)
            var to = schedule.GetNextOccurrence(from);
            var diff = to - from;
            Assert.Equal(expected, diff.Days);
        }

        [Test]
        public void Monthly_on_first_of_month()
        {
            var cron = CronTemplates.Monthly();
            var schedule = CronTemplates.Parse(cron);
            var diff = CompareTwoCronOccurrences(schedule);
            Assert.True(diff.Days == 30 || diff.Days == 31);
        }

        private static TimeSpan CompareTwoCronOccurrences(CrontabSchedule schedule)
        {
            var from = schedule.GetNextOccurrence(DateTime.Now); // <-- throw this one away to normalize
            from = schedule.GetNextOccurrence(from);
            var to = schedule.GetNextOccurrence(from);
            var diff = to - from;
            return diff;
        }
    }
}

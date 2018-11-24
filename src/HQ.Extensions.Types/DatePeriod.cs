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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace HQ.Extensions.Types
{
    /// <summary>
    ///     A period of time used in date calculations.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Frequency} x {Quantifier}")]
    public struct DatePeriod
    {
        ///<param name="frequency">The period frequency, paired with the quantifier</param>
        ///<param name="quantifier">The period quantifier, paired with frequency</param>
        public DatePeriod(DatePeriodFrequency frequency, int quantifier) : this()
        {
            Frequency = frequency;
            Quantifier = quantifier;
        }

        /// <summary>
        ///     The period frequency, paired with <see cref="Quantifier" />
        /// </summary>
        public DatePeriodFrequency Frequency { get; private set; }

        /// <summary>
        ///     The period quantifier, paired with <see cref="Frequency" />
        /// </summary>
        public int Quantifier { get; private set; }

        /// <summary>
        ///     A period of time occurring every day
        /// </summary>
        public static DatePeriod Daily
        {
            get { return new DatePeriod(DatePeriodFrequency.Days, 1); }
        }

        /// <summary>
        ///     A period of time occurring every month
        /// </summary>
        public static DatePeriod Monthly
        {
            get { return new DatePeriod(DatePeriodFrequency.Months, 1); }
        }

        /// <summary>
        ///     A period of time occurring every other month
        /// </summary>
        public static DatePeriod BiMonthly
        {
            get { return new DatePeriod(DatePeriodFrequency.Months, 2); }
        }

        /// <summary>
        ///     A period of time occurring every week
        /// </summary>
        public static DatePeriod Weekly
        {
            get { return new DatePeriod(DatePeriodFrequency.Weeks, 1); }
        }

        /// <summary>
        ///     A period of time occurring every other week
        /// </summary>
        public static DatePeriod BiWeekly
        {
            get { return new DatePeriod(DatePeriodFrequency.Weeks, 2); }
        }

        /// <summary>
        ///     A period of time occurring every year
        /// </summary>
        public static DatePeriod Annually
        {
            get { return new DatePeriod(DatePeriodFrequency.Years, 1); }
        }

        /// <summary>
        ///     A period of time occurring every other year
        /// </summary>
        public static DatePeriod BiAnnually
        {
            get { return new DatePeriod(DatePeriodFrequency.Years, 2); }
        }

        /// <summary>
        ///     Gets the date occurrences in this period, between a start and end date.
        ///     If an occurrence falls on a weekend, it is deferred to the start
        ///     of the next week.
        /// </summary>
        /// <param name="start">The starting date.</param>
        /// <param name="end">The ending date.</param>
        /// <param name="skipWeekends">If true, occurrences scheduled for a weekend are deferred to the following weekday</param>
        /// <returns>A list of dates representing period occurrences.</returns>
        public IEnumerable<DateTime> GetOccurrences(DateTime start, DateTime end, bool skipWeekends = true)
        {
            var calendar = CultureInfo.CurrentCulture.Calendar;

            switch (Frequency)
            {
                case DatePeriodFrequency.Days:
                    return GetOccurrences(DateInterval.Days, this, calendar, start, end, skipWeekends);
                case DatePeriodFrequency.Weeks:
                    return GetOccurrences(DateInterval.Weeks, this, calendar, start, end, skipWeekends);
                case DatePeriodFrequency.Months:
                    return GetOccurrences(DateInterval.Months, this, calendar, start, end, skipWeekends);
                case DatePeriodFrequency.Years:
                    return GetOccurrences(DateInterval.Years, this, calendar, start, end, skipWeekends);
                default:
                    throw new ArgumentException("Frequency");
            }
        }

        private static IEnumerable<DateTime> GetOccurrences(DateInterval interval, DatePeriod period, Calendar calendar,
            DateTime start, DateTime end, bool skipWeekends = true)
        {
            var difference = DateSpan.GetDifference(interval, start, end) / period.Quantifier;

            if (start.Kind == DateTimeKind.Utc)
            {
                start = start.ToLocalTime();
            }

            for (var i = 0; i < difference; i++)
            {
                switch (period.Frequency)
                {
                    case DatePeriodFrequency.Days:
                        var days = calendar.AddDays(start, period.Quantifier * i);
                        yield return DeferOccurrenceFallingOnWeekend(calendar, days, skipWeekends);
                        break;
                    case DatePeriodFrequency.Weeks:
                        var weeks = calendar.AddWeeks(start, period.Quantifier * i);
                        yield return DeferOccurrenceFallingOnWeekend(calendar, weeks, skipWeekends);
                        break;
                    case DatePeriodFrequency.Months:
                        var months = calendar.AddMonths(start, period.Quantifier * i);
                        yield return DeferOccurrenceFallingOnWeekend(calendar, months, skipWeekends);
                        break;
                    case DatePeriodFrequency.Years:
                        var years = calendar.AddYears(start, period.Quantifier * i);
                        yield return DeferOccurrenceFallingOnWeekend(calendar, years, skipWeekends);
                        break;
                    default:
                        throw new ArgumentException("Frequency");
                }
            }
        }

        private static DateTime DeferOccurrenceFallingOnWeekend(Calendar calendar, DateTime occurrence,
            bool skipWeekends = true)
        {
            if (skipWeekends)
            {
                if (occurrence.DayOfWeek == DayOfWeek.Saturday)
                {
                    occurrence = calendar.AddDays(occurrence, 2);
                }

                if (occurrence.DayOfWeek == DayOfWeek.Sunday)
                {
                    occurrence = calendar.AddDays(occurrence, 1);
                }
            }

            return occurrence.ToUniversalTime();
        }
    }
}

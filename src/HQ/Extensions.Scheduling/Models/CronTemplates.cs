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
using NCrontab;

namespace HQ.Extensions.Scheduling.Models
{
    public static class CronTemplates
    {
        private static readonly CrontabSchedule.ParseOptions ParseOptions = new CrontabSchedule.ParseOptions { IncludingSeconds = true };

        public static string Secondly(int seconds = 0)
        {
            return $"{ValueOrStar(seconds)} * * * * *";
        }

        public static string Minutely(int minutes = 0, int atSecond = 0)
        {
            return $"{atSecond} {ValueOrStar(minutes)} * * * *";
        }

        public static string Hourly(int hours = 0, int atMinute = 0, int atSecond = 0)
        {
            return $"{atSecond} {atMinute} {ValueOrStar(hours)} * * *";
        }

        public static string Daily(int days, int atHour = 0, int atMinute = 0, int atSecond = 0)
        {
            return $"{atSecond} {atMinute} {atHour} */{days} * *";
        }

        public static string Weekly(DayOfWeek onDay, int atHour = 0, int atMinute = 0, int atSecond = 0)
        {
            return $"{atSecond} {atMinute} {atHour} * * {(int) onDay}";
        }

        public static string Weekly(int atHour = 0, int atMinute = 0, int atSecond = 0, params DayOfWeek[] onDays)
        {
            if (onDays.Length == 0)
            {
                return null;
            }

            var expression = Weekly(onDays[0], atHour, atMinute, atSecond);

            for (var i = 1; i < onDays.Length; i++)
            {
                expression = $"{expression},{(int) onDays[i]}";
            }

            return expression;
        }

        public static string Monthly(int onDay = 1, int atHour = 0, int atMinute = 0, int atSecond = 0)
        {
            return $"{atSecond} {atMinute} {atHour} {onDay} * *";
        }

        private static string ValueOrStar(int value)
        {
            return value == 0 ? "*" : $"*/{value}";
        }

        public static CrontabSchedule Parse(string expression)
        {
            return CrontabSchedule.Parse(expression, ParseOptions);
        }

        public static bool TryParse(string expression, out CrontabSchedule schedule)
        {
            schedule = CrontabSchedule.TryParse(expression, ParseOptions);
            return schedule != null;
        }
    }
}

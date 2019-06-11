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

namespace HQ.Extensions.Scheduling.Models
{
    public class CronTemplates
    {
        public static string Minutely(int minutes = 0)
        {
            return $"{ValueOrStar(minutes)} * * * *";
        }

        public static string Hourly(int hours = 0, int atMinute = 0)
        {
            return $"{atMinute} {ValueOrStar(hours)} * * *";
        }

        public static string Daily(int days, int atHour = 0, int atMinute = 0)
        {
            return $"{atMinute} {atHour} */{days} * *";
        }

        public static string WeekDaily(DayOfWeek onDay, int atHour = 0, int atMinute = 0)
        {
            return $"{atMinute} {atHour} * * {(int) onDay}";
        }

        public static string WeekDaily(int atHour = 0, int atMinute = 0, params DayOfWeek[] onDays)
        {
            if (onDays.Length == 0)
            {
                return null;
            }

            var expression = WeekDaily(onDays[0], atHour, atMinute);

            for (var i = 1; i < onDays.Length; i++)
            {
                expression = $"{expression},{(int) onDays[i]}";
            }

            return expression;
        }

        public static string Monthly(int onDay = 1, int atHour = 0, int atMinute = 0)
        {
            return $"{atMinute} {atHour} {onDay} * *";
        }

        private static string ValueOrStar(int value)
        {
            return value == 0 ? "*" : "*/" + value;
        }
    }
}

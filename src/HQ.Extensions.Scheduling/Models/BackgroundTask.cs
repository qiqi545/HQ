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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using HQ.Data.Contracts.DataAnnotations;
using NCrontab;
using Newtonsoft.Json;

namespace HQ.Extensions.Scheduling.Models
{
    public class BackgroundTask
    {
        public int Id { get; set; }
        public int Priority { get; set; }
        public int Attempts { get; set; }
        public string Handler { get; set; }
        public DateTimeOffset RunAt { get; set; }
        public TimeSpan? MaximumRuntime { get; set; }
        public int? MaximumAttempts { get; set; }
        public bool? DeleteOnSuccess { get; set; }
        public bool? DeleteOnFailure { get; set; }
        public bool? DeleteOnError { get; set; }
        public string LastError { get; set; }
        public DateTimeOffset? FailedAt { get; set; }
        public DateTimeOffset? SucceededAt { get; set; }
        public DateTimeOffset? LockedAt { get; set; }
        public string LockedBy { get; set; }

        public string Expression { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset? End { get; set; }
        public bool ContinueOnSuccess { get; set; } = true;
        public bool ContinueOnFailure { get; set; } = true;
        public bool ContinueOnError { get; set; } = true;

        [Computed]
        public DateTimeOffset CreatedAt { get; set; }

        [NotMapped]
        public List<string> Tags { get; set; } = new List<string>();

        [Computed]
        public bool RunningOvertime => IsRunningOvertime();

        private bool IsRunningOvertime()
        {
            if (!LockedAt.HasValue)
            {
                return false;
            }

            if (!MaximumRuntime.HasValue)
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow;
            var elapsed = LockedAt.Value - now;

            // overtime = 125% of maximum runtime
            var overage = (long) (MaximumRuntime.Value.Ticks / 0.25f);
            var overtime = MaximumRuntime.Value + new TimeSpan(overage);

            if (elapsed >= overtime)
            {
                return true;
            }

            return false;
        }

        [JsonIgnore, Computed] public DateTimeOffset? NextOccurrence => GetNextOccurence();
        [JsonIgnore, Computed] public DateTimeOffset? LastOccurrence => GetLastOccurrence();
        [JsonIgnore, Computed] public IEnumerable<DateTimeOffset> AllOccurrences => GetAllOccurrences();

        [JsonIgnore, Computed]
        public bool HasValidExpression
        {
            get
            {
                var schedule = TryParseCron();
                return schedule != null;
            }
        }

        private IEnumerable<DateTimeOffset> GetAllOccurrences()
        {
            if (!HasValidExpression)
            {
                return Enumerable.Empty<DateTimeOffset>();
            }

            if (!End.HasValue)
            {
                throw new ArgumentException("You cannot request all occurrences of an infinite series", nameof(End));
            }

            return GetFiniteSeriesOccurrences(End.Value);
        }

        private DateTimeOffset? GetNextOccurence()
        {
            if (!HasValidExpression)
            {
                return null;
            }

            // important: never iterate occurrences, the series could be inadvertently huge (i.e. 100 years of seconds)
            return End == null
                ? GetNextOccurrenceInInfiniteSeries()
                : GetFiniteSeriesOccurrences(End.Value).FirstOrDefault();
        }

        private DateTimeOffset? GetLastOccurrence()
        {
            if (!HasValidExpression)
            {
                return null;
            }

            if (!End.HasValue)
            {
                throw new ArgumentException("You cannot request the last occurrence of an infinite series",
                    nameof(End));
            }

            return GetFiniteSeriesOccurrences(End.Value).Last();
        }

        private DateTimeOffset? GetNextOccurrenceInInfiniteSeries()
        {
            var schedule = TryParseCron();
            if (schedule == null)
            {
                return null;
            }

            var nextOccurrence = schedule.GetNextOccurrence(RunAt.UtcDateTime);
            return new DateTimeOffset(nextOccurrence);
        }

        private IEnumerable<DateTimeOffset> GetFiniteSeriesOccurrences(DateTimeOffset end)
        {
            var schedule = TryParseCron();
            if (schedule == null)
            {
                return Enumerable.Empty<DateTimeOffset>();
            }

            var nextOccurrences = schedule.GetNextOccurrences(RunAt.UtcDateTime, end.UtcDateTime);
            var occurrences = nextOccurrences.Select(o => new DateTimeOffset(o));
            return occurrences;
        }

        private CrontabSchedule TryParseCron()
        {
            return string.IsNullOrWhiteSpace(Expression) ? null :
                !CronTemplates.TryParse(Expression, out var schedule) ? null : schedule;
        }
    }
}

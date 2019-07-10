using System;
using System.Collections.Generic;
using HQ.Data.Contracts.Attributes;
using HQ.Extensions.Scheduling.Models;
using HQ.Integration.DocumentDb.Sql;
using Newtonsoft.Json;

namespace HQ.Integration.DocumentDb.Scheduling
{
    public class BackgroundTaskDocument : DocumentBase<BackgroundTaskDocument>
    {
        [JsonConstructor]
        public BackgroundTaskDocument() { }

        public BackgroundTaskDocument(BackgroundTask task)
        {
            TaskId = task.Id;
            Priority = task.Priority;
            Attempts = task.Attempts;
            Handler = task.Handler;
            RunAt = task.RunAt;
            MaximumRuntime = task.MaximumRuntime;
            MaximumAttempts = task.MaximumAttempts;
            DeleteOnSuccess = task.DeleteOnSuccess;
            DeleteOnFailure = task.DeleteOnFailure;
            DeleteOnError = task.DeleteOnError;
            LastError = task.LastError;
            FailedAt = task.FailedAt;
            SucceededAt = task.SucceededAt;
            LockedAt = task.LockedAt;
            LockedBy = task.LockedBy;

            Expression = task.Expression;
            Start = task.Start;
            End = task.End;
            ContinueOnSuccess = task.ContinueOnSuccess;
            ContinueOnFailure = task.ContinueOnFailure;
            ContinueOnError = task.ContinueOnError;
            
            Tags = task.Tags ?? new List<string>();
        }

        public static implicit operator BackgroundTask (BackgroundTaskDocument document)
        {
            var task = new BackgroundTask();

            task.Id = document.TaskId;
            task.Priority = document.Priority;
            task.Attempts = document.Attempts;
            task.Handler = document.Handler;
            task.RunAt = document.RunAt;
            task.MaximumRuntime = document.MaximumRuntime;
            task.MaximumAttempts = document.MaximumAttempts;
            task.DeleteOnSuccess = document.DeleteOnSuccess;
            task.DeleteOnFailure = document.DeleteOnFailure;
            task.DeleteOnError = document.DeleteOnError;
            task.LastError = document.LastError;
            task.FailedAt = document.FailedAt;
            task.SucceededAt = document.SucceededAt;
            task.LockedAt = document.LockedAt;
            task.LockedBy = document.LockedBy;

            task.Expression = document.Expression;
            task.Start = document.Start;
            task.End = document.End;
            task.ContinueOnSuccess = document.ContinueOnSuccess;
            task.ContinueOnFailure = document.ContinueOnFailure;
            task.ContinueOnError = document.ContinueOnError;

            task.Tags = document.Tags;
            task.CreatedAt = document.Timestamp;
            return task;
        }

        [AutoIncrement]
        public int TaskId { get; set; }
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
        public List<string> Tags { get; set; } = new List<string>();
    }
}

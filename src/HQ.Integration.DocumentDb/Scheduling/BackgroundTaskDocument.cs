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
using HQ.Data.Contracts.DataAnnotations;
using HQ.Extensions.Scheduling.Models;
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
			CorrelationId = task.CorrelationId;
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

			Data = task.Data;
			Tags = task.Tags ?? new List<string>();
		}

		[AutoIncrement] public int TaskId { get; set; }
		public Guid CorrelationId { get; set; }

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

		public string Data { get; set; }

		public bool ContinueOnSuccess { get; set; } = true;
		public bool ContinueOnFailure { get; set; } = true;
		public bool ContinueOnError { get; set; } = true;
		public List<string> Tags { get; set; } = new List<string>();

		public static implicit operator BackgroundTask(BackgroundTaskDocument document)
		{
			var task = new BackgroundTask
			{
				Id = document.TaskId,
				CorrelationId = document.CorrelationId,
				Priority = document.Priority,
				Attempts = document.Attempts,
				Handler = document.Handler,
				RunAt = document.RunAt,
				MaximumRuntime = document.MaximumRuntime,
				MaximumAttempts = document.MaximumAttempts,
				DeleteOnSuccess = document.DeleteOnSuccess,
				DeleteOnFailure = document.DeleteOnFailure,
				DeleteOnError = document.DeleteOnError,
				LastError = document.LastError,
				FailedAt = document.FailedAt,
				SucceededAt = document.SucceededAt,
				LockedAt = document.LockedAt,
				LockedBy = document.LockedBy,
				Expression = document.Expression,
				Start = document.Start,
				End = document.End,
				ContinueOnSuccess = document.ContinueOnSuccess,
				ContinueOnFailure = document.ContinueOnFailure,
				ContinueOnError = document.ContinueOnError,
				Tags = document.Tags,
				CreatedAt = document.Timestamp,
				Data = document.Data
			};
			return task;
		}
	}
}
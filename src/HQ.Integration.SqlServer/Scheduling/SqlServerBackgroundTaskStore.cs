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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HQ.Common;
using HQ.Data.SessionManagement;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Integration.SqlServer.Scheduling
{
	public class SqlServerBackgroundTaskStore : IBackgroundTaskStore
	{
		private static readonly List<string> NoTags = new List<string>();

		private readonly ISafeLogger<SqlServerBackgroundTaskStore> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IServerTimestampService _timestamps;
		private readonly string _schema;
		private readonly string _tablePrefix;

		public SqlServerBackgroundTaskStore(IServiceProvider serviceProvider, IServerTimestampService timestamps, string schema = "dbo",
			string tablePrefix = nameof(BackgroundTask), ISafeLogger<SqlServerBackgroundTaskStore> logger = null)
		{
			_serviceProvider = serviceProvider;
			_timestamps = timestamps;
			_schema = schema;
			_tablePrefix = tablePrefix;
			_logger = logger;
		}

		internal string TaskTable => $"[{_schema}].[{_tablePrefix}]";
		internal string TagTable => $"[{_schema}].[{_tablePrefix}_Tag]";
		internal string TagsTable => $"[{_schema}].[{_tablePrefix}_Tags]";

		public async Task<IEnumerable<BackgroundTask>> GetByAnyTagsAsync(params string[] tags)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out _);
			return await GetByAnyTagsWithTagsAsync(tags, db, t);
		}

		public async Task<IEnumerable<BackgroundTask>> GetByAllTagsAsync(params string[] tags)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out _);
			return await GetByAllTagsWithTagsAsync(tags, db, t);
		}

		public Task<BackgroundTask> GetByIdAsync(int id)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out _);
			var task = GetByIdWithTagsAsync(id, db, t);
			return task;
		}

		public async Task<IEnumerable<BackgroundTask>> GetHangingTasksAsync()
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out _);
			var locked = await GetLockedTasksWithTagsAsync(db, t);
			return locked.Where(st => st.RunningOvertime);
		}

		public Task<IEnumerable<BackgroundTask>> GetAllAsync()
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out _);
			return GetAllWithTagsAsync(db, t);
		}

		public async Task<bool> SaveAsync(BackgroundTask task)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			if (task.Id == 0)
				await InsertBackgroundTaskAsync(task, db, t);
			else
				await UpdateBackgroundTaskAsync(task, db, t);

			await UpdateTagMapping(task, db, t);

			if (owner)
				CommitTransaction(t);
			return true;
		}

		public async Task<bool> DeleteAsync(BackgroundTask task)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			var sql = $@"
-- Primary relationship:
DELETE FROM {TagsTable} WHERE BackgroundTaskId = @Id;
DELETE FROM {TaskTable} WHERE Id = @Id;

-- Remove any orphaned tags:
DELETE FROM {TagTable}
WHERE NOT EXISTS (SELECT 1 FROM {TagsTable} st WHERE {TagTable}.Id = st.TagId)
";
			await db.ExecuteAsync(sql, task, t);
			if (owner)
				CommitTransaction(t);
			return true;
		}

		public async Task<IEnumerable<BackgroundTask>> LockNextAvailableAsync(int readAhead)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			var tasks = await GetUnlockedTasksWithTags(readAhead, db, t);
			// ReSharper disable once PossibleMultipleEnumeration
			if (tasks.Any())
				// ReSharper disable once PossibleMultipleEnumeration
				await LockTasksAsync(tasks, db, t);
			if (owner)
				CommitTransaction(t);
			// ReSharper disable once PossibleMultipleEnumeration
			return tasks;
		}

		private IDbTransaction BeginTransaction(SqlConnection connection, out bool owner)
		{
			var db = GetDataConnection();

			if (db.Transaction != null)
			{
				_logger?.Debug(() => "Transaction is already initiated; we are not the owner.");
				owner = false;
				return db.Transaction;
			}

			_logger?.Debug(() => "Owner-initiated transaction occurred.");
			var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
			db.Transaction = transaction;
			owner = true;
			return transaction;
		}

		private void CommitTransaction(IDbTransaction t)
		{
			t.Commit();
			var db = GetDataConnection();
			db.Transaction = null;
			_logger?.Debug(() => "Owner-committed transaction occurred.");
		}

		private async Task<IEnumerable<BackgroundTask>> GetLockedTasksWithTagsAsync(IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE 
    {TaskTable}.LockedAt IS NOT NULL
ORDER BY
    {TagTable}.Name ASC    
";
			return await QueryWithSplitOnTagsAsync(db, t, sql);
		}

		private async Task<IEnumerable<BackgroundTask>> GetUnlockedTasksWithTags(int readAhead, IDbConnection db,
			IDbTransaction t)
		{
			// None locked, failed or succeeded, must be due, ordered by due time then priority
			var sql = $@"
SELECT TOP {{0}} st.* FROM {TaskTable} st
WHERE
    st.[LockedAt] IS NULL 
AND
    st.[FailedAt] IS NULL 
AND 
    st.[SucceededAt] IS NULL
AND 
    (st.[RunAt] <= GETUTCDATE())
ORDER BY 
    st.[RunAt], 
    st.[Priority] ASC
";
			var matchSql = string.Format(sql, readAhead);

			var matches = await db.QueryAsync<BackgroundTask>(matchSql, transaction: t);

			// ReSharper disable once PossibleMultipleEnumeration
			if (!matches.Any())
				// ReSharper disable once PossibleMultipleEnumeration
				return matches;

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN @Ids
ORDER BY {TagTable}.Name ASC
";
			// ReSharper disable once PossibleMultipleEnumeration
			var ids = matches.Select(m => m.Id);

			return await QueryWithSplitOnTagsAsync(db, t, fetchSql, new {Ids = ids});
		}

		private async Task<BackgroundTask> GetByIdWithTagsAsync(int id, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id = @Id
ORDER BY {TagTable}.Name ASC
";
			BackgroundTask task = null;
			await db.QueryAsync<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
			{
				task ??= s;
				if (tag != null)
					task.Tags.Add(tag);
				return task;
			}, new {Id = id}, splitOn: "Name", transaction: t);

			return task;
		}

		private async Task<IEnumerable<BackgroundTask>> GetAllWithTagsAsync(IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
ORDER BY {TagTable}.Name ASC
";
			return await QueryWithSplitOnTagsAsync(db, t, sql);
		}

		private async Task<IEnumerable<BackgroundTask>> GetByAnyTagsWithTagsAsync(IEnumerable tags, IDbConnection db,
			IDbTransaction t)
		{
			var matchSql = $@"
SELECT st.*
FROM {TagsTable} stt, {TaskTable} st, {TagTable} t
WHERE stt.TagId = t.Id
AND t.Name IN @Tags
AND st.Id = stt.BackgroundTaskId
GROUP BY 
st.[Priority], st.Attempts, st.Handler, st.RunAt, st.MaximumRuntime, st.MaximumAttempts, st.DeleteOnSuccess, st.DeleteOnFailure, st.DeleteOnError, st.Expression, st.Start, st.[End], st.ContinueOnSuccess, st.ContinueOnFailure, st.ContinueOnError, st.Data,
st.Id, st.LastError, st.FailedAt, st.SucceededAt, st.LockedAt, st.LockedBy, st.CreatedAt,
t.Name
";
			var matches = db.Query<BackgroundTask>(matchSql, new {Tags = tags}, t).ToList();

			if (!matches.Any())
				return matches;

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN @Ids
ORDER BY {TagTable}.Name ASC
";
			var ids = matches.Select(m => m.Id);

			return await QueryWithSplitOnTagsAsync(db, t, fetchSql, new {Ids = ids});
		}

		private async Task<IEnumerable<BackgroundTask>> GetByAllTagsWithTagsAsync(IReadOnlyCollection<string> tags,
			IDbConnection db, IDbTransaction t)
		{
			var matchSql = $@"
SELECT st.*
FROM {TagsTable} stt, {TaskTable} st, {TagTable} t
WHERE stt.TagId = t.Id
AND t.Name IN @Tags
AND st.Id = stt.BackgroundTaskId
GROUP BY 
st.[Priority], st.Attempts, st.Handler, st.RunAt, st.MaximumRuntime, st.MaximumAttempts, st.DeleteOnSuccess, st.DeleteOnFailure, st.DeleteOnError, st.Expression, st.Start, st.[End], st.ContinueOnSuccess, st.ContinueOnFailure, st.ContinueOnError, st.Data,
st.Id, st.LastError, st.FailedAt, st.SucceededAt, st.LockedAt, st.LockedBy, st.CreatedAt
HAVING COUNT (st.Id) = @Count
";
			var matches = db.Query<BackgroundTask>(matchSql, new {Tags = tags, tags.Count}, t).ToList();

			if (!matches.Any())
				return matches;

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN @Ids
ORDER BY {TagTable}.Name ASC
";
			var ids = matches.Select(m => m.Id);

			return await QueryWithSplitOnTagsAsync(db, t, fetchSql, new {Ids = ids});
		}

		private static async Task<IEnumerable<BackgroundTask>> QueryWithSplitOnTagsAsync(IDbConnection db,
			IDbTransaction t, string sql, object data = null)
		{
			var lookup = new Dictionary<int, BackgroundTask>();

			await db.QueryAsync<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
			{
				if (!lookup.TryGetValue(s.Id, out var task))
					lookup.Add(s.Id, task = s);
				if (tag != null)
					task.Tags.Add(tag);
				return task;
			}, data, splitOn: "Name", transaction: t);

			var result = lookup.Values.ToList();
			return result;
		}

		private async Task UpdateBackgroundTaskAsync(BackgroundTask task, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
UPDATE {TaskTable} 
SET 
    Priority = @Priority, 
    Attempts = @Attempts, 
    Handler = @Handler, 
    RunAt = @RunAt, 
    MaximumRuntime = @MaximumRuntime, 
    MaximumAttempts = @MaximumAttempts, 
    DeleteOnSuccess = @DeleteOnSuccess,
    DeleteOnFailure = @DeleteOnFailure,
    DeleteOnError = @DeleteOnError,
    Expression = @Expression, 
    Start = @Start, 
    [End] = @End,
    ContinueOnSuccess = @ContinueOnSuccess,
    ContinueOnFailure = @ContinueOnFailure,
    ContinueOnError = @ContinueOnError,
    LastError = @LastError,
    FailedAt = @FailedAt, 
    SucceededAt = @SucceededAt, 
    LockedAt = @LockedAt, 
    LockedBy = @LockedBy
WHERE 
    Id = @Id
";
			await db.ExecuteAsync(sql, task, t);
		}

		private async Task InsertBackgroundTaskAsync(BackgroundTask task, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
INSERT INTO {TaskTable} 
    (Priority, Attempts, Handler, RunAt, MaximumRuntime, MaximumAttempts, DeleteOnSuccess, DeleteOnFailure, DeleteOnError, Expression, Start, [End], ContinueOnSuccess, ContinueOnFailure, ContinueOnError, [Data]) 
VALUES
    (@Priority, @Attempts, @Handler, @RunAt, @MaximumRuntime, @MaximumAttempts, @DeleteOnSuccess, @DeleteOnFailure, @DeleteOnError, @Expression, @Start, @End, @ContinueOnSuccess, @ContinueOnFailure, @ContinueOnError, @Data);

SELECT MAX(Id) FROM {TaskTable};
";
			task.Id = await db.QuerySingleAsync<int>(sql, task, t);
			task.CreatedAt =
				(await db.QueryAsync<DateTimeOffset>($"SELECT CreatedAt FROM {TaskTable} WHERE Id = @Id", task, t))
				.Single();
		}

		private async Task LockTasksAsync(IEnumerable<BackgroundTask> tasks, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
UPDATE {TaskTable}  
SET 
    LockedAt = @Now, 
    LockedBy = @User 
WHERE Id IN 
    @Ids
";
			var now = _timestamps.GetCurrentTime();
			var user = LockedIdentity.Get();

			var array = tasks as BackgroundTask[] ?? tasks.ToArray();
			var ids = array.Select(task => task.Id);

			// ReSharper disable once PossibleMultipleEnumeration
			var updated = await db.ExecuteAsync(sql, new {Now = now, Ids = ids, User = user}, t);
			Debug.Assert(updated == array.Length, "did not update the expected number of tasks");

			// ReSharper disable once PossibleMultipleEnumeration
			foreach (var task in array)
			{
				task.LockedAt = now;
				task.LockedBy = user;
			}
		}

		private async Task UpdateTagMapping(BackgroundTask task, IDbConnection db, IDbTransaction t)
		{
			var source = task.Tags ?? NoTags;

			if (source == NoTags || source.Count == 0)
			{
				await db.ExecuteAsync($"DELETE FROM {TagsTable} WHERE [BackgroundTaskId] = @Id", task, t);
				return;
			}

			// normalize for storage
			var normalized = source.Select(st => st.Trim().Replace(" ", "-").Replace("'", "\""));

			// values = VALUES @Tags
			// sql server only permits 1000 values per statement
			foreach (var tags in normalized.Split(1000))
			{
				var upsertSql = $@"
-- upsert tags
MERGE {TagTable}
USING (VALUES {{0}}) AS Pending (Name) 
ON {TagTable}.Name = Pending.Name 
WHEN NOT MATCHED THEN
    INSERT (Name) VALUES (Pending.Name)
;

-- sync tag mappings
MERGE {TagsTable}
USING
    (SELECT @BackgroundTaskId AS BackgroundTaskId, Id AS TagId 
       FROM {TagTable} 
      WHERE Name IN @Tags) AS Pending (BackgroundTaskId, TagId) 
ON {TagsTable}.BackgroundTaskId = Pending.BackgroundTaskId 
AND {TagsTable}.TagId = Pending.TagId 
WHEN NOT MATCHED BY SOURCE AND {TagsTable}.BackgroundTaskId = @BackgroundTaskId THEN
    DELETE 
WHEN NOT MATCHED THEN 
    INSERT (BackgroundTaskId,TagId) VALUES (Pending.BackgroundTaskId, Pending.TagId)
;
";
				var values = tags.Select(tag => $"('{tag}')");
				var sql = string.Format(upsertSql, string.Join(",", values));
				await db.ExecuteAsync(sql, new {BackgroundTaskId = task.Id, Tags = tags}, t);
			}
		}
		
		private IDbConnection GetDbConnection()
		{
			// IMPORTANT: DI is out of our hands, here.
			var connection = GetDataConnection();
			return connection.Current;
		}

		private IDataConnection<BackgroundTaskBuilder> GetDataConnection()
		{
			// IMPORTANT: DI is out of our hands, here.
			return _serviceProvider.GetRequiredService<IDataConnection<BackgroundTaskBuilder>>();
		}
	}
}
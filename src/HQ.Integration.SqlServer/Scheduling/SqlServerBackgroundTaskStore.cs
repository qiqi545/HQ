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

		#region Query

		public Task<IEnumerable<BackgroundTask>> GetByAnyTagsAsync(params string[] tags)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			try
			{
				var tasks = GetByAnyTagsWithTags(tags, db, t);
				return Task.FromResult(tasks);
			}
			finally
			{
				if (owner)
					CommitTransaction(t);
			}
		}

		public Task<IEnumerable<BackgroundTask>> GetByAllTagsAsync(params string[] tags)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			try
			{
				var tasks = GetByAllTagsWithTags(tags, db, t);
				return Task.FromResult(tasks);
			}
			finally
			{
				if (owner)
					CommitTransaction(t);
			}
		}

		public Task<BackgroundTask> GetByIdAsync(int id)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			try
			{
				var task = GetByIdWithTags(id, db, t);
				return Task.FromResult(task);
			}
			finally
			{
				if (owner)
					CommitTransaction(t);
			}
		}

		public Task<IEnumerable<BackgroundTask>> GetHangingTasksAsync()
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			try
			{
				var locked = GetLockedTasksWithTags(db, t);
				var tasks = locked.Where(st => st.IsRunningOvertime(this));
				return Task.FromResult(tasks);
			}
			finally
			{
				if(owner)
					CommitTransaction(t);
			}
		}

		public Task<IEnumerable<BackgroundTask>> GetAllAsync()
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			try
			{
				var tasks = GetAllWithTags(db, t);
				return Task.FromResult(tasks);
			}
			finally
			{
				if(owner)
					CommitTransaction(t);
			}
		}

		#endregion

		#region Mutate

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

		public Task<IEnumerable<BackgroundTask>> LockNextAvailableAsync(int readAhead)
		{
			var db = GetDbConnection();
			var t = BeginTransaction((SqlConnection) db, out var owner);
			
			var tasks = GetUnlockedTasksWithTags(readAhead, db, t);

			if (tasks.Count == 0)
				return Task.FromResult((IEnumerable<BackgroundTask>) tasks);

			LockTasks(tasks, db, t);

			if (owner)
				CommitTransaction(t);

			return Task.FromResult((IEnumerable<BackgroundTask>) tasks);
		}

		#endregion

		private IDbTransaction BeginTransaction(SqlConnection connection, out bool owner)
		{
			var db = GetDataConnection();

			if (db.Transaction == null)
			{
				lock (this)
				{
					if (db.Transaction == null)
					{
						var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
						db.SetTransaction(transaction);
						owner = true;

						_logger?.Debug(() => "Owner-initiated transaction occurred.");
						return transaction;
					}
				}
			}
			
			_logger?.Debug(() => "Transaction is already initiated; we are not the owner.");
			owner = false;
			return db.Transaction;
		}

		private void CommitTransaction(IDbTransaction t)
		{
			t.Commit();
			var db = GetDataConnection();
			db.SetTransaction(null);
			_logger?.Debug(() => "Owner-committed transaction occurred.");
		}

		#region Transacted Calls (Non-Async)

		private IEnumerable<BackgroundTask> GetLockedTasksWithTags(IDbConnection db, IDbTransaction t)
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
			return QueryWithSplitOnTags(db, t, sql);
		}

		private IList<BackgroundTask> GetUnlockedTasksWithTags(int readAhead, IDbConnection db, IDbTransaction t)
		{
			var now = GetTaskTimestamp();

			// None locked, failed or succeeded, must be due, ordered by due time then priority
			var sql = $@"
SELECT TOP {readAhead} st.* FROM {TaskTable} st
WHERE
    st.[LockedAt] IS NULL 
AND
    st.[FailedAt] IS NULL 
AND 
    st.[SucceededAt] IS NULL
AND 
    (st.[RunAt] <= @Now)
ORDER BY 
    st.[RunAt], 
    st.[Priority] ASC
";

			var matches = db.Query<BackgroundTask>(sql, new { Now = now }, t).AsList();
			if (matches.Count == 0)
			{
				_logger.Debug(() => "No unlocked tasks found.");
				return matches;
			}

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN @Ids
ORDER BY {TagTable}.Name ASC
";
			
			var ids = GetTaskIds(matches);
			var tasks = QueryWithSplitOnTags(db, t, fetchSql, new {Ids = ids});
			return tasks;
		}

		private BackgroundTask GetByIdWithTags(int id, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id = @Id
ORDER BY {TagTable}.Name ASC
";
			BackgroundTask task = null;
			db.Query<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
			{
				task ??= s;
				if (tag != null)
					task.Tags.Add(tag);
				return task;
			}, new {Id = id}, splitOn: "Name", transaction: t);

			return task;
		}

		private IEnumerable<BackgroundTask> GetAllWithTags(IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
ORDER BY {TagTable}.Name ASC
";
			var tasks = QueryWithSplitOnTags(db, t, sql);
			return tasks;
		}

		private IEnumerable<BackgroundTask> GetByAnyTagsWithTags(IEnumerable tags, IDbConnection db, IDbTransaction t)
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
			var matches = db.Query<BackgroundTask>(matchSql, new {Tags = tags}, t).AsList();
			if (matches.Count == 0)
				return matches;

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN @Ids
ORDER BY {TagTable}.Name ASC
";

			var ids = GetTaskIds(matches);
			var tasks = QueryWithSplitOnTags(db, t, fetchSql, new {Ids = ids}).AsList();
			return tasks;
		}

		private IEnumerable<BackgroundTask> GetByAllTagsWithTags(IReadOnlyCollection<string> tags, IDbConnection db, IDbTransaction t)
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
			
			var matches = db.Query<BackgroundTask>(matchSql, new {Tags = tags, tags.Count}, t).AsList();
			if (matches.Count == 0)
				return matches;

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN @Ids
ORDER BY {TagTable}.Name ASC
";

			var ids = GetTaskIds(matches);
			var tasks = QueryWithSplitOnTags(db, t, fetchSql, new {Ids = ids}).AsList();
			return tasks;
		}

		private static IList<BackgroundTask> QueryWithSplitOnTags(IDbConnection db, IDbTransaction t, string sql, object data = null)
		{
			var lookup = new Dictionary<int, BackgroundTask>();

			db.Query<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
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
			task.CreatedAt = GetTaskTimestamp();

			var sql = $@"
INSERT INTO {TaskTable} 
    (Priority, Attempts, Handler, RunAt, MaximumRuntime, MaximumAttempts, DeleteOnSuccess, DeleteOnFailure, DeleteOnError, Expression, Start, [End], ContinueOnSuccess, ContinueOnFailure, ContinueOnError, [Data], [CreatedAt]) 
VALUES
    (@Priority, @Attempts, @Handler, @RunAt, @MaximumRuntime, @MaximumAttempts, @DeleteOnSuccess, @DeleteOnFailure, @DeleteOnError, @Expression, @Start, @End, @ContinueOnSuccess, @ContinueOnFailure, @ContinueOnError, @Data, @CreatedAt);

SELECT MAX([Id]) AS [Id] FROM {TaskTable}
";
			task.Id = await db.QuerySingleAsync<int>(sql, task, t);
		}

		private void LockTasks(IEnumerable<BackgroundTask> tasks, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
UPDATE {TaskTable}  
SET 
    LockedAt = @Now, 
    LockedBy = @User 
WHERE Id IN 
    @Ids;
";
			var now = GetTaskTimestamp();
			var user = LockedIdentity.Get();

			var matches = tasks.AsList();
			var ids = GetTaskIds(matches);

			_logger.Debug(() => "Locking {TaskCount} tasks", matches.Count);
			var updated = db.Execute(sql, new {Now = now, Ids = ids, User = user}, t);
			if(updated != matches.Count)
				_logger.Warn(()=> "Did not lock the expected number of tasks");

			foreach (var task in matches)
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
				var values = tags.Enumerate(tag => $"('{tag}')");
				var sql = string.Format(upsertSql, string.Join(",", values));
				await db.ExecuteAsync(sql, new {BackgroundTaskId = task.Id, Tags = tags}, t);
			}
		}

		#endregion

		private WeakReference<IDbConnection> _lastDbConnection;
		private IDbConnection GetDbConnection()
		{
			// IMPORTANT: DI is out of our hands, here.
			var connection = GetDataConnection();
			var current = connection.Current;
			
			if (_lastDbConnection != null && _lastDbConnection.TryGetTarget(out var target) && target == current)
				_logger.Debug(() => "IDbConnection is pre-initialized.");
			_lastDbConnection = new WeakReference<IDbConnection>(current, false);

			return current;
		}

		private WeakReference<IDataConnection> _lastDataConnection;
		private IDataConnection<BackgroundTaskBuilder> GetDataConnection()
		{
			// IMPORTANT: DI is out of our hands, here.
			var connection = _serviceProvider.GetRequiredService<IDataConnection<BackgroundTaskBuilder>>();
			
			if (_lastDataConnection != null && _lastDataConnection.TryGetTarget(out var target) && target == connection)
				_logger.Debug(()=> "IDataConnection is pre-initialized.");
			_lastDataConnection = new WeakReference<IDataConnection>(connection, false);
			
			return connection;
		}

		public DateTimeOffset GetTaskTimestamp()
		{
			return _timestamps.GetCurrentTime();
		}

		private static IEnumerable<int> GetTaskIds(List<BackgroundTask> tasks)
		{
			foreach (var id in tasks.Enumerate(x => x.Id))
				yield return id;
		}
	}
}
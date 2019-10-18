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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HQ.Common;
using HQ.Data.SessionManagement;
using HQ.Extensions.Scheduling;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Integration.Sqlite.Scheduling
{
	public class SqliteBackgroundTaskStore : IBackgroundTaskStore
	{
		private static readonly List<string> NoTags = new List<string>();

		private readonly IDataConnection _db;
		private readonly IServerTimestampService _timestamps;
		private readonly string _tablePrefix;

		public SqliteBackgroundTaskStore(IDataConnection<BackgroundTaskBuilder> db,
			IServerTimestampService timestamps,
			string tablePrefix = "BackgroundTask")
		{
			_db = db;
			_timestamps = timestamps;
			_tablePrefix = tablePrefix;
		}

		internal string TaskTable => $"\"{_tablePrefix}\"";
		internal string TagTable => $"\"{_tablePrefix}_Tag\"";
		internal string TagsTable => $"\"{_tablePrefix}_Tags\"";

		public async Task<IEnumerable<BackgroundTask>> GetByAnyTagsAsync(params string[] tags)
		{
			var db = _db.Current;
			using var t = BeginTransaction(_db.Current, out _);
			return await GetByAnyTagsWithTagsAsync(tags, db, t);
		}

		public async Task<IEnumerable<BackgroundTask>> GetByAllTagsAsync(params string[] tags)
		{
			var db = _db.Current;
			using var t = BeginTransaction(_db.Current, out _);
			return await GetByAllTagsWithTagsAsync(tags, db, t);
		}

		public async Task<IEnumerable<BackgroundTask>> LockNextAvailableAsync(int readAhead)
		{
			var db = _db.Current;

			List<BackgroundTask> tasks;
			using (var t = BeginTransaction(_db.Current, out var owner))
			{
				tasks = await GetUnlockedTasksWithTagsAsync(readAhead, db, t);
				if (tasks.Any())
					await LockTasksAsync(tasks, db, t);

				if (owner)
					t.Commit();
			}

			return tasks;
		}

		public DateTimeOffset GetTaskTimestamp()
		{
			return _timestamps.GetCurrentTime().ToUniversalTime();
		}

		public async Task<BackgroundTask> GetByIdAsync(int id)
		{
			var db = _db.Current;
			BackgroundTask task;
			using (var t = BeginTransaction(_db.Current, out _))
				task = await GetByIdWithTagsAsync(id, db, t);
			return task;
		}

		public async Task<IEnumerable<BackgroundTask>> GetHangingTasksAsync()
		{
			var db = _db.Current;
			IEnumerable<BackgroundTask> locked;
			using (var t = BeginTransaction(_db.Current, out _))
				locked = await GetLockedTasksWithTagsAsync(db, t);

			return locked.Where(st => st.IsRunningOvertime(this)).ToList();
		}

		public async Task<IEnumerable<BackgroundTask>> GetAllAsync()
		{
			var db = _db.Current;
			using var t = BeginTransaction(_db.Current, out _);
			return await GetAllWithTagsAsync(db, t);
		}

		public async Task<bool> SaveAsync(BackgroundTask task)
		{
			using (var t = BeginTransaction(_db.Current, out var owner))
			{
				if (task.Id == 0)
				{
					await InsertBackgroundTaskAsync(task, _db.Current, t);
				}
				else
				{
					await UpdateBackgroundTaskAsync(task, _db.Current, t);
				}

				await UpdateTagMappingAsync(task, _db.Current, t);

				if (owner)
					t.Commit();
			}

			return true;
		}

		public async Task<bool> DeleteAsync(BackgroundTask task)
		{
			var db = _db.Current;
			using (var t = BeginTransaction(_db.Current, out var owner))
			{
				var sql = $@"
-- Primary relationship:
DELETE FROM {TagsTable} WHERE BackgroundTaskId = :Id;
DELETE FROM {TaskTable} WHERE Id = :Id;

-- Remove any orphaned tags:
DELETE FROM {TagTable}
WHERE NOT EXISTS (SELECT 1 FROM {TagsTable} st WHERE {TagTable}.Id = st.TagId)
";
				await db.ExecuteAsync(sql, task, t);
				if (owner)
					t.Commit();
			}

			return true;
		}

		private IDbTransaction BeginTransaction(IDbConnection db, out bool owner)
		{
			var transaction = db.BeginTransaction(IsolationLevel.Serializable);
			_db.SetTransaction(transaction);
			owner = true;
			return transaction;
		}

		private async Task InsertBackgroundTaskAsync(BackgroundTask task, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
INSERT INTO {TaskTable} 
    (Priority, Attempts, Handler, RunAt, MaximumRuntime, MaximumAttempts, DeleteOnSuccess, DeleteOnFailure, DeleteOnError, Expression, Start, [End], ContinueOnSuccess, ContinueOnFailure, ContinueOnError) 
VALUES
    (:Priority, :Attempts, :Handler, :RunAt, :MaximumRuntime, :MaximumAttempts, :DeleteOnSuccess, :DeleteOnFailure, :DeleteOnError, :Expression, :Start, :End, :ContinueOnSuccess, :ContinueOnFailure, :ContinueOnError);

SELECT MAX(Id) FROM {TaskTable};
";
			task.Id = (await db.QueryAsync<int>(sql, task, t)).Single();
			var createdAtString = await db.QuerySingleAsync<string>($"SELECT \"CreatedAt\" FROM {TaskTable} WHERE \"Id\" = :Id", new {task.Id}, t);
			task.CreatedAt = DateTimeOffset.Parse(createdAtString);
		}

		private async Task UpdateBackgroundTaskAsync(BackgroundTask task, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
UPDATE {TaskTable} 
SET 
    Priority = :Priority, 
    Attempts = :Attempts, 
    Handler = :Handler, 
    RunAt = :RunAt, 
    MaximumRuntime = :MaximumRuntime, 
    MaximumAttempts = :MaximumAttempts, 
    DeleteOnSuccess = :DeleteOnSuccess,
    DeleteOnFailure = :DeleteOnFailure,
    DeleteOnError = :DeleteOnError,
    Expression = :Expression, 
    Start = :Start, 
    [End] = :End,
    ContinueOnSuccess = :ContinueOnSuccess,
    ContinueOnFailure = :ContinueOnFailure,
    ContinueOnError = :ContinueOnError,
    LastError = :LastError,
    FailedAt = :FailedAt, 
    SucceededAt = :SucceededAt, 
    LockedAt = :LockedAt, 
    LockedBy = :LockedBy
WHERE 
    Id = @Id
";
			await db.ExecuteAsync(sql, task, t);
		}

		private async Task LockTasksAsync(IReadOnlyCollection<BackgroundTask> tasks, IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
UPDATE {TaskTable}  
SET 
    LockedAt = :Now, 
    LockedBy = :User 
WHERE Id IN 
    :Ids
";
			var now = GetTaskTimestamp();
			var user = LockedIdentity.Get();

			await db.ExecuteAsync(sql, new {Now = now, Ids = tasks.Select(task => task.Id), User = user}, t);

			foreach (var task in tasks)
			{
				task.LockedAt = now;
				task.LockedBy = user;
			}
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

		private async Task<List<BackgroundTask>> GetUnlockedTasksWithTagsAsync(int readAhead, IDbConnection db,
			IDbTransaction t)
		{
			// None locked, failed or succeeded, must be due, ordered by due time then priority
			var sql = $@"
SELECT st.* FROM {TaskTable} st
WHERE
    st.LockedAt IS NULL 
AND
    st.FailedAt IS NULL 
AND 
    st.SucceededAt IS NULL
AND 
    (st.RunAt <= DATETIME('now'))
ORDER BY 
    st.RunAt, 
    st.Priority ASC
LIMIT {readAhead}
";
			var matchSql = string.Format(sql, readAhead);

			var matches = (await db.QueryAsync<BackgroundTask>(matchSql, transaction: t)).ToList();

			if (!matches.Any())
			{
				return matches;
			}

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN :Ids
ORDER BY {TagTable}.Name ASC
";
			var ids = matches.Select(m => m.Id);

			return await QueryWithSplitOnTagsAsync(db, t, fetchSql, new {Ids = ids});
		}

		private async Task<BackgroundTask> GetByIdWithTagsAsync(int id, IDbConnection db, IDbTransaction t)
		{
			var taskTable = TaskTable;
			var tagTable = TagTable;
			var tagsTable = TagsTable;

			var sql = $@"
SELECT {taskTable}.*, {tagTable}.Name FROM {taskTable}
LEFT JOIN {tagsTable} ON {tagsTable}.BackgroundTaskId = {taskTable}.Id
LEFT JOIN {tagTable} ON {tagsTable}.TagId = {tagTable}.Id
WHERE {taskTable}.Id = :Id
ORDER BY {tagTable}.Name ASC
";
			BackgroundTask task = null;
			await db.QueryAsync<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
			{
				task ??= s;
				if (tag != null)
				{
					task.Tags.Add(tag);
				}

				return task;
			}, new {Id = id}, splitOn: "Name", transaction: t);

			return task;
		}

		private async Task<IList<BackgroundTask>> GetAllWithTagsAsync(IDbConnection db, IDbTransaction t)
		{
			var sql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
ORDER BY {TagTable}.Name ASC
";
			return await QueryWithSplitOnTagsAsync(db, t, sql);
		}

		private async Task<IList<BackgroundTask>> GetByAnyTagsWithTagsAsync(IReadOnlyCollection<string> tags,
			IDbConnection db, IDbTransaction t = null)
		{
			var matchSql = $@"
SELECT st.*
FROM {TagsTable} stt, {TaskTable} st, {TagTable} t
WHERE stt.TagId = t.Id
AND t.Name IN :Tags
AND st.Id = stt.BackgroundTaskId
GROUP BY 
st.[Priority], st.Attempts, st.Handler, st.RunAt, st.MaximumRuntime, st.MaximumAttempts, st.DeleteOnSuccess, st.DeleteOnFailure, st.DeleteOnError, st.Expression, st.Start, st.End, st.ContinueOnSuccess, st.ContinueOnFailure, st.ContinueOnError, 
st.Id, st.LastError, st.FailedAt, st.SucceededAt, st.LockedAt, st.LockedBy, st.CreatedAt,
t.Name
";
			var matches = db.Query<BackgroundTask>(matchSql, new {Tags = tags}, t).ToList();

			if (!matches.Any())
			{
				return matches;
			}

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN :Ids
ORDER BY {TagTable}.Name ASC
";
			var ids = matches.Select(m => m.Id);

			return await QueryWithSplitOnTagsAsync(db, t, fetchSql, new {Ids = ids});
		}

		private async Task<IList<BackgroundTask>> GetByAllTagsWithTagsAsync(IReadOnlyCollection<string> tags,
			IDbConnection db, IDbTransaction t)
		{
			var matchSql = $@"
SELECT st.*
FROM {TagsTable} stt, {TaskTable} st, {TagTable} t
WHERE stt.TagId = t.Id
AND t.Name IN :Tags
AND st.Id = stt.BackgroundTaskId
GROUP BY 
st.[Priority], st.Attempts, st.Handler, st.RunAt, st.MaximumRuntime, st.MaximumAttempts, st.DeleteOnSuccess, st.DeleteOnFailure, st.DeleteOnError, st.Expression, st.Start, st.[End], st.ContinueOnSuccess, st.ContinueOnFailure, st.ContinueOnError, 
st.Id, st.LastError, st.FailedAt, st.SucceededAt, st.LockedAt, st.LockedBy, st.CreatedAt
HAVING COUNT (st.Id) = :Count
";
			var matches = (await db.QueryAsync<BackgroundTask>(matchSql, new {Tags = tags, tags.Count}, t)).ToList();

			if (!matches.Any())
			{
				return matches;
			}

			var fetchSql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
WHERE {TaskTable}.Id IN :Ids
ORDER BY {TagTable}.Name ASC
";
			var ids = matches.Select(m => m.Id);

			return await QueryWithSplitOnTagsAsync(db, t, fetchSql, new {Ids = ids});
		}

		private static async Task<List<BackgroundTask>> QueryWithSplitOnTagsAsync(IDbConnection db, IDbTransaction t,
			string sql,
			object data = null)
		{
			var lookup = new Dictionary<int, BackgroundTask>();
			await db.QueryAsync<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
			{
				if (!lookup.TryGetValue(s.Id, out var task))
				{
					lookup.Add(s.Id, task = s);
				}

				if (tag != null)
				{
					task.Tags.Add(tag);
				}

				return task;
			}, data, splitOn: "Name", transaction: t);

			var result = lookup.Values.ToList();
			return result;
		}

		private async Task UpdateTagMappingAsync(BackgroundTask task, IDbConnection db, IDbTransaction t)
		{
			var source = task.Tags ?? NoTags;

			await db.ExecuteAsync($"DELETE FROM {TagsTable} WHERE BackgroundTaskId = @Id", task, t);

			if (source == NoTags || source.Count == 0)
				return;

			// normalize for storage
			var normalized = source.Select(st => st.Trim().Replace(" ", "-").Replace("'", "\""));

			foreach (var tags in normalized.Split(1000))
			{
				foreach (var tag in tags)
				{
					await db.ExecuteAsync($@"INSERT OR IGNORE INTO {TagTable} (Name) VALUES (:Name);", new {Name = tag},
						t);

					var id = db.QuerySingle<int>($"SELECT Id FROM {TagTable} WHERE Name = :Name", new {Name = tag}, t);

					await db.ExecuteAsync(
						$@"INSERT INTO {TagsTable} (BackgroundTaskId, TagId) VALUES (:BackgroundTaskId, :TagId)",
						new {BackgroundTaskId = task.Id, TagId = id}, t);
				}
			}
		}
	}
}
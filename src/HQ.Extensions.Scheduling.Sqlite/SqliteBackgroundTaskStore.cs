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
using Dapper;
using HQ.Common;
using HQ.Data.SessionManagement;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Extensions.Scheduling.Sqlite
{
    public class SqliteBackgroundTaskStore : IBackgroundTaskStore
    {
        private static readonly List<string> NoTags = new List<string>();

        private readonly IDataConnection _db;
        private readonly string _tablePrefix;
        
        public SqliteBackgroundTaskStore(IDataConnection db, string tablePrefix = "BackgroundTask")
        {
            _db = db;
            _tablePrefix = tablePrefix;
        }

        internal string TaskTable => $"\"{_tablePrefix}\"";
        internal string TagTable => $"\"{_tablePrefix}_Tag\"";
        internal string TagsTable => $"\"{_tablePrefix}_Tags\"";

        private IDbTransaction BeginTransaction(IDbConnection db, out bool owner)
        {
            var transaction = db.BeginTransaction(IsolationLevel.Serializable);
            _db.Transaction = transaction;
            owner = true;
            return transaction;
        }

        public IList<BackgroundTask> GetByAnyTags(params string[] tags)
        {
            var db = _db.Current;
            using (var t = BeginTransaction(_db.Current, out _))
            {
                return GetByAnyTagsWithTags(tags, db, t);
            }
        }

        public IList<BackgroundTask> GetByAllTags(params string[] tags)
        {
            var db = _db.Current;
            using (var t = BeginTransaction(_db.Current, out _))
            {
                return GetByAllTagsWithTags(tags, db, t);
            }
        }

        public IList<BackgroundTask> LockNextAvailable(int readAhead)
        {
            var db = _db.Current;

            List<BackgroundTask> tasks;
            using (var t = BeginTransaction(_db.Current, out var owner))
            {
                tasks = GetUnlockedTasksWithTags(readAhead, db, t);
                if (tasks.Any())
                {
                    LockTasks(tasks, db, t);
                }
                if(owner)
                    t.Commit();
            }

            return tasks;
        }

        public BackgroundTask GetById(int id)
        {
            var db = _db.Current;
            BackgroundTask task;
            using (var t = BeginTransaction(_db.Current, out _))
            {
                task = GetByIdWithTags(id, db, t);
            }
            return task;
        }

        public IList<BackgroundTask> GetHangingTasks()
        {
            var db = _db.Current;
            IEnumerable<BackgroundTask> locked;
            using (var t = BeginTransaction(_db.Current, out _))
            {
                locked = GetLockedTasksWithTags(db, t);
            }

            return locked.Where(st => st.RunningOvertime).ToList();
        }

        public IList<BackgroundTask> GetAll()
        {
            var db = _db.Current;
            using (var t = BeginTransaction(_db.Current, out _))
            {
                return GetAllWithTags(db, t);
            }
        }

        public bool Save(BackgroundTask task)
        {
            using (var t = BeginTransaction(_db.Current, out var owner))
            {
                if (task.Id == 0)
                {
                    InsertBackgroundTask(task, _db.Current, t);
                }
                else
                {
                    UpdateBackgroundTask(task, _db.Current, t);
                }

                UpdateTagMapping(task, _db.Current, t);

                if(owner)
                    t.Commit();
            }

            return true;
        }

        private void InsertBackgroundTask(BackgroundTask task, IDbConnection db, IDbTransaction t)
        {
            var sql = $@"
INSERT INTO {TaskTable} 
    (Priority, Attempts, Handler, RunAt, MaximumRuntime, MaximumAttempts, DeleteOnSuccess, DeleteOnFailure, DeleteOnError, Expression, Start, [End], ContinueOnSuccess, ContinueOnFailure, ContinueOnError) 
VALUES
    (:Priority, :Attempts, :Handler, :RunAt, :MaximumRuntime, :MaximumAttempts, :DeleteOnSuccess, :DeleteOnFailure, :DeleteOnError, :Expression, :Start, :End, :ContinueOnSuccess, :ContinueOnFailure, :ContinueOnError);

SELECT MAX(Id) FROM {TaskTable};
";
            task.Id = db.Query<int>(sql, task, t).Single();
            var createdAtString = db.Query<string>($"SELECT \"CreatedAt\" FROM {TaskTable} WHERE Id = :Id", task, t).Single();
            task.CreatedAt = DateTimeOffset.Parse(createdAtString);
        }

        private void UpdateBackgroundTask(BackgroundTask task, IDbConnection db, IDbTransaction t)
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
            db.Execute(sql, task, t);
        }
        
        public bool Delete(BackgroundTask task)
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
                db.Execute(sql, task, t);
                if(owner)
                    t.Commit();
            }

            return true;
        }

        private void LockTasks(IReadOnlyCollection<BackgroundTask> tasks, IDbConnection db, IDbTransaction t)
        {
            var sql = $@"
UPDATE {TaskTable}  
SET 
    LockedAt = :Now, 
    LockedBy = :User 
WHERE Id IN 
    :Ids
";
            var now = DateTime.Now;
            var user = LockedIdentity.Get();

            db.Execute(sql, new {Now = now, Ids = tasks.Select(task => task.Id), User = user}, t);

            foreach (var task in tasks)
            {
                task.LockedAt = now;
                task.LockedBy = user;
            }
        }

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

        private List<BackgroundTask> GetUnlockedTasksWithTags(int readAhead, IDbConnection db, IDbTransaction t)
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

            var matches = db.Query<BackgroundTask>(matchSql, transaction: t).ToList();

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

            return QueryWithSplitOnTags(db, t, fetchSql, new {Ids = ids});
        }

        private BackgroundTask GetByIdWithTags(int id, IDbConnection db, IDbTransaction t)
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
            db.Query<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
            {
                task = task ?? s;
                if (tag != null)
                {
                    task.Tags.Add(tag);
                }
                return task;
            }, new {Id = id}, splitOn: "Name", transaction: t);

            return task;
        }

        private IList<BackgroundTask> GetAllWithTags(IDbConnection db, IDbTransaction t)
        {
            var sql = $@"
SELECT {TaskTable}.*, {TagTable}.Name FROM {TaskTable}
LEFT JOIN {TagsTable} ON {TagsTable}.BackgroundTaskId = {TaskTable}.Id
LEFT JOIN {TagTable} ON {TagsTable}.TagId = {TagTable}.Id
ORDER BY {TagTable}.Name ASC
";
            return QueryWithSplitOnTags(db, t, sql);
        }
        private IList<BackgroundTask> GetByAnyTagsWithTags(IReadOnlyCollection<string> tags, IDbConnection db, IDbTransaction t = null)
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

            return QueryWithSplitOnTags(db, t, fetchSql, new {Ids = ids});
        }

        private IList<BackgroundTask> GetByAllTagsWithTags(IReadOnlyCollection<string> tags, IDbConnection db, IDbTransaction t)
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
            var matches = db.Query<BackgroundTask>(matchSql, new {Tags = tags, tags.Count}, t).ToList();

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

            return QueryWithSplitOnTags(db, t, fetchSql, new {Ids = ids});
        }
        
        private static List<BackgroundTask> QueryWithSplitOnTags(IDbConnection db, IDbTransaction t, string sql,
            object data = null)
        {
            var lookup = new Dictionary<int, BackgroundTask>();
            db.Query<BackgroundTask, string, BackgroundTask>(sql, (s, tag) =>
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

        private void UpdateTagMapping(BackgroundTask task, IDbConnection db, IDbTransaction t)
        {
            var source = task.Tags ?? NoTags;

            db.Execute($"DELETE FROM {TagsTable} WHERE BackgroundTaskId = @Id", task, t);

            if (source == NoTags || source.Count == 0)
                return;

            // normalize for storage
            var normalized = source.Select(st => st.Trim().Replace(" ", "-").Replace("'", "\""));

            foreach (var tags in normalized.Split(1000))
            {
                foreach(var tag in tags)
                {
                    db.Execute($@"INSERT OR IGNORE INTO {TagTable} (Name) VALUES (:Name);", new { Name = tag }, t);

                    var id = db.QuerySingle<int>($"SELECT Id FROM {TagTable} WHERE Name = :Name", new { Name = tag }, t);

                    db.Execute($@"INSERT INTO {TagsTable} (BackgroundTaskId, TagId) VALUES (:BackgroundTaskId, :TagId)",
                        new {BackgroundTaskId = task.Id, TagId = id}, t);
                }
            }
        }
    }
}

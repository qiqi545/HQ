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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HQ.Common;

namespace HQ.Extensions.Scheduling.Models
{
    public class InMemoryBackgroundTaskStore : IBackgroundTaskStore
    {
        private static int _identity;

        private readonly IDictionary<int, HashSet<BackgroundTask>> _tasks;

        public InMemoryBackgroundTaskStore()
        {
            _tasks = new ConcurrentDictionary<int, HashSet<BackgroundTask>>();
        }

        public IList<BackgroundTask> GetByAnyTags(params string[] tags)
        {
            var all = GetAll();

            var query = all.Where(a => { return tags.Any(tag => a.Tags.Contains(tag)); });

            return query.ToList();
        }

        public IList<BackgroundTask> GetByAllTags(params string[] tags)
        {
            var all = GetAll();

            var query = all.Where(a => { return tags.All(tag => a.Tags.Contains(tag)); });

            return query.ToList();
        }

        public void Save(BackgroundTask task)
        {
            if (!_tasks.TryGetValue(task.Priority, out var tasks))
            {
                task.CreatedAt = DateTimeOffset.UtcNow; ;
                _tasks.Add(task.Priority, tasks = new HashSet<BackgroundTask>());
            }

            if (tasks.All(t => t.Id != task.Id))
            {
                tasks.Add(task);
                task.Id = ++_identity;
            }
        }

        public void Delete(BackgroundTask task)
        {
            if (_tasks.TryGetValue(task.Priority, out var tasks))
            {
                tasks.Remove(task);
            }
        }

        public IList<BackgroundTask> GetAndLockNextAvailable(int readAhead)
        {
            var all = _tasks.SelectMany(t => t.Value);

            // None locked, failed or succeeded, must be due, ordered by due time then priority
            var now = DateTimeOffset.UtcNow;

            var query = all
                .Where(t => !t.FailedAt.HasValue && !t.SucceededAt.HasValue && !t.LockedAt.HasValue)
                .Where(t => t.RunAt <= now)
                .OrderBy(t => t.RunAt)
                .ThenBy(t => t.Priority)
                .MaybeList();

            var tasks = (query.Count > readAhead ? query.Take(readAhead) : query).MaybeList();

            // Lock tasks:
            if (tasks.Any())
            {
                foreach (var scheduledTask in tasks)
                {
                    scheduledTask.LockedAt = now;
                    scheduledTask.LockedBy = LockedIdentity.Get();
                }
            }

            return tasks.ToList();
        }

        public BackgroundTask GetById(int id)
        {
            return _tasks.SelectMany(t => t.Value).SingleOrDefault(t => t.Id == id);
        }

        public IList<BackgroundTask> GetHangingTasks()
        {
            return GetAll().Where(t => t.RunningOvertime).ToList();
        }

        public IList<BackgroundTask> GetAll()
        {
            IEnumerable<BackgroundTask> all = _tasks.SelectMany(t => t.Value).OrderBy(t => t.Priority);

            return all.ToList();
        }
    }
}

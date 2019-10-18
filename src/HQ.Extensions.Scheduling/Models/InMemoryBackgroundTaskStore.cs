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
using System.Threading.Tasks;
using HQ.Common;

namespace HQ.Extensions.Scheduling.Models
{
	public class InMemoryBackgroundTaskStore : IBackgroundTaskStore
	{
		private static int _identity;

		private readonly IDictionary<int, HashSet<BackgroundTask>> _tasks;
		private readonly IServerTimestampService _timestamps;

		public InMemoryBackgroundTaskStore(IServerTimestampService timestamps)
		{
			_timestamps = timestamps;
			_tasks = new ConcurrentDictionary<int, HashSet<BackgroundTask>>();
		}

		public async Task<IEnumerable<BackgroundTask>> GetByAnyTagsAsync(params string[] tags)
		{
			var all = await GetAllAsync();

			var query = all.Where(a => { return tags.Any(tag => a.Tags.Contains(tag)); });

			return query.ToList();
		}

		public async Task<IEnumerable<BackgroundTask>> GetByAllTagsAsync(params string[] tags)
		{
			var all = await GetAllAsync();

			var query = all.Where(a => { return tags.All(tag => a.Tags.Contains(tag)); });

			return query.ToList();
		}

		public Task<bool> SaveAsync(BackgroundTask task)
		{
			if (!_tasks.TryGetValue(task.Priority, out var tasks))
			{
				task.CreatedAt = GetTaskTimestamp();
				_tasks.Add(task.Priority, tasks = new HashSet<BackgroundTask>());
			}

			if (tasks.All(t => t.Id != task.Id))
			{
				tasks.Add(task);
				task.Id = ++_identity;
				return Task.FromResult(true);
			}

			return Task.FromResult(false);
		}

		public Task<bool> DeleteAsync(BackgroundTask task)
		{
			if (_tasks.TryGetValue(task.Priority, out var tasks))
			{
				tasks.Remove(task);
				return Task.FromResult(true);
			}

			return Task.FromResult(false);
		}

		public Task<IEnumerable<BackgroundTask>> LockNextAvailableAsync(int readAhead)
		{
			var all = _tasks.SelectMany(t => t.Value);

			// None locked, failed or succeeded, must be due, ordered by due time then priority
			var now = GetTaskTimestamp();

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

			return Task.FromResult(tasks.AsEnumerable());
		}

		public DateTimeOffset GetTaskTimestamp()
		{
			return _timestamps.GetCurrentTime().ToUniversalTime();
		}

		public Task<BackgroundTask> GetByIdAsync(int id)
		{
			return Task.FromResult(_tasks.SelectMany(t => t.Value).SingleOrDefault(t => t.Id == id));
		}

		public async Task<IEnumerable<BackgroundTask>> GetHangingTasksAsync()
		{
			var tasks = await GetAllAsync();

			return tasks.Where(t => t.IsRunningOvertime(this)).ToList();
		}

		public Task<IEnumerable<BackgroundTask>> GetAllAsync()
		{
			IEnumerable<BackgroundTask> all = _tasks.SelectMany(t => t.Value).OrderBy(t => t.Priority);

			return Task.FromResult(all);
		}

		public void Clear()
		{
			_tasks.Clear();
		}
	}
}
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Integration.DocumentDb.Scheduling
{
    public class DocumentBackgroundTaskStore : IBackgroundTaskStore
    {
        private readonly IDocumentDbRepository<BackgroundTaskDocument> _repository;
        private readonly IServerTimestampService _timestamps;

        public DocumentBackgroundTaskStore(IDocumentDbRepository<BackgroundTaskDocument> repository, IServerTimestampService timestamps)
        {
            _repository = repository;
            _timestamps = timestamps;
        }

        public async Task<BackgroundTask> GetByIdAsync(int id)
        {
            var task =  await _repository.RetrieveSingleOrDefaultAsync(x => x.TaskId == id);
            // ReSharper disable once UseNullPropagation (implicit conversion will fail)
            // ReSharper disable once ConvertIfStatementToReturnStatement (implicit conversion will fail)
            if (task == null)
                return null;
            return task;
        }

        public async Task<IEnumerable<BackgroundTask>> GetAllAsync()
        {
            var tasks = await _repository.RetrieveAsync();

            return tasks.Select(x => (BackgroundTask) x);
        }

        public async Task<IEnumerable<BackgroundTask>> GetByAllTagsAsync(params string[] tags)
        {
            // DB doesn't support All expression, so we have to project Any then filter it client-side
            // ReSharper disable once ConvertClosureToMethodGroup (DB doesn't recognize method groups)
            var tasks = await _repository.RetrieveAsync(x => x.Tags.Any(t => tags.Contains(t)));

            // Reduce "any" to "all" on the client
            var all = tasks.Where(x => tags.All(t => x.Tags.Contains(t)));

            return all.Select(x => (BackgroundTask)x);
        }

        public async Task<IEnumerable<BackgroundTask>> GetByAnyTagsAsync(params string[] tags)
        {
            // ReSharper disable once ConvertClosureToMethodGroup (DB doesn't recognize method groups)
            var tasks = await _repository.RetrieveAsync(x => x.Tags.Any(t => tags.Contains(t)));

            return tasks.Select(x => (BackgroundTask)x);
        }

        public async Task<IEnumerable<BackgroundTask>> GetHangingTasksAsync()
        {
            var tasks = await _repository.RetrieveAsync(x => x.LockedAt.HasValue);

            return tasks.Select(x => (BackgroundTask) x).Where(x => x.RunningOvertime);
        }

        public async Task<bool> SaveAsync(BackgroundTask task)
        {
            var document = new BackgroundTaskDocument(task);

            if (task.Id == 0)
            {
                await _repository.CreateAsync(document);
                task.Id = document.TaskId;
                return true;
            }

            var existing = await _repository.RetrieveSingleOrDefaultAsync(x => x.TaskId == task.Id);
            if (existing == null)
            {
                await _repository.CreateAsync(document);
                return true;
            }

            document.Id = existing.Id;

            await _repository.UpdateAsync(existing.Id, document);
            return true;
        }

        public async Task<bool> DeleteAsync(BackgroundTask task)
        {
            var document = await _repository.RetrieveSingleOrDefaultAsync(x => x.TaskId == task.Id);
            if (document == null)
                return false;

            await _repository.DeleteAsync(document.Id);
            return true;
        }

        public async Task<IEnumerable<BackgroundTask>> LockNextAvailableAsync(int readAhead)
        {
            var now = _timestamps.GetCurrentTime();

            var tasks = (await _repository.RetrieveAsync(x =>
                x.LockedAt == null &&
                x.FailedAt == null &&
                x.SucceededAt == null &&
                x.RunAt <= now)).ToList();

            foreach (var task in tasks)
            {
                task.LockedAt = now;
                task.LockedBy = LockedIdentity.Get();
                await _repository.UpdateAsync(task.Id, task);
            }

            return tasks
                .OrderBy(x => x.RunAt)
                .ThenBy(x => x.Priority)
                .Select(x => (BackgroundTask) x);
        }
    }
}

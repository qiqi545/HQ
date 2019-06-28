using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Sql.DocumentDb;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Extensions.Scheduling.DocumentDb
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
            var tasks = await _repository.RetrieveAsync(x => x.Tags.All(tags.Contains));

            return tasks.Select(x => (BackgroundTask)x);
        }

        public async Task<IEnumerable<BackgroundTask>> GetByAnyTagsAsync(params string[] tags)
        {
            var tasks = await _repository.RetrieveAsync(x => x.Tags.Any(tags.Contains));

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
                !x.LockedAt.HasValue &&
                !x.FailedAt.HasValue &&
                !x.SucceededAt.HasValue &&
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

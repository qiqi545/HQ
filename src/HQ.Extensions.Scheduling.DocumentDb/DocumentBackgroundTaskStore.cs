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
            return await _repository.RetrieveSingleOrDefaultAsync(x => x.TaskId == id);
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
            if (task.Id == 0)
            {
                await _repository.CreateAsync(new BackgroundTaskDocument(task));
                return true;
            }

            var document = await _repository.RetrieveSingleOrDefaultAsync(x => x.TaskId == task.Id);
            if (document == null)
            {
                await _repository.CreateAsync(new BackgroundTaskDocument(task));
                return true;
            }

            await _repository.UpdateAsync(document.Id, new BackgroundTaskDocument(task));
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

using System;
using System.Collections.Generic;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Extensions.Scheduling.DocumentDb
{
    public class DocumentBackgroundTaskStore : IBackgroundTaskStore
    {
        public BackgroundTask GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IList<BackgroundTask> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<BackgroundTask> GetByAllTags(params string[] tags)
        {
            throw new NotImplementedException();
        }

        public IList<BackgroundTask> GetByAnyTags(params string[] tags)
        {
            throw new NotImplementedException();
        }

        public IList<BackgroundTask> GetHangingTasks()
        {
            throw new NotImplementedException();
        }

        public bool Save(BackgroundTask task)
        {
            throw new NotImplementedException();
        }

        public bool Delete(BackgroundTask task)
        {
            throw new NotImplementedException();
        }

        public IList<BackgroundTask> LockNextAvailable(int readAhead)
        {
            throw new NotImplementedException();
        }
    }
}

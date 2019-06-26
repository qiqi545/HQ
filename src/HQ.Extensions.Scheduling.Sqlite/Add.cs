using System;

namespace HQ.Extensions.Scheduling.Sqlite
{
    public static class Add
    {
        public static BackgroundTaskBuilder AddSqliteBackgroundTaskStore(this BackgroundTaskBuilder builder)
        {
            return builder;
        }
    }
}

using Xunit;

namespace HQ.Extensions.Scheduling.Tests.Sqlite
{
    public class SqliteBackgroundTaskStoreTests : BackgroundTaskStoreTests, IClassFixture<SchedulingSqliteFixture>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public SqliteBackgroundTaskStoreTests(SchedulingSqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

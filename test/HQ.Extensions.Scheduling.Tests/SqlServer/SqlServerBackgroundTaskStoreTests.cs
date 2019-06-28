using Xunit;

namespace HQ.Extensions.Scheduling.Tests.SqlServer
{
    public class SqlServerBackgroundTaskStoreTests : BackgroundTaskStoreTests, IClassFixture<SchedulingSqlServerFixture>
    {
        private readonly SchedulingSqlServerFixture _fixture;

        // ReSharper disable once SuggestBaseTypeForParameter
        public SqlServerBackgroundTaskStoreTests(SchedulingSqlServerFixture fixture) : base(CreateServiceProvider(fixture))
        {
            _fixture = fixture;
        }
    }
}

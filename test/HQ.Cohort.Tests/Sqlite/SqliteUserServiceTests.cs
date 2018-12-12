using Xunit;

namespace HQ.Cohort.Tests.Sqlite
{
    public class SqliteUserServiceTests : UserServiceTests, IClassFixture<SqliteFixture>
    {
        public SqliteUserServiceTests(SqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

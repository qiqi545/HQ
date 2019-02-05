using Xunit;

namespace HQ.Extensions.Identity.Tests.Sqlite
{
    public class SqliteUserServiceTests : UserServiceTests, IClassFixture<SqliteFixture>
    {
        public SqliteUserServiceTests(SqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

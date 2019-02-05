using Xunit;

namespace HQ.Platform.Identity.Tests.Sqlite
{
    public class SqliteRoleServiceTests : RoleServiceTests, IClassFixture<SqliteFixture>
    {
        public SqliteRoleServiceTests(SqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

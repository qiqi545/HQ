using Xunit;

namespace HQ.Extensions.Identity.Tests.Sqlite
{
    public class SqliteRoleServiceTests : RoleServiceTests, IClassFixture<SqliteFixture>
    {
        public SqliteRoleServiceTests(SqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

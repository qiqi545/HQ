using Xunit;

namespace HQ.Cohort.Tests.Sqlite
{
    public class SqliteRoleServiceTests : RoleServiceTests, IClassFixture<SqliteFixture>
    {
        public SqliteRoleServiceTests(SqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

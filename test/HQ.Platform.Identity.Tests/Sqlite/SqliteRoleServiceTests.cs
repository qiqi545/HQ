using HQ.Test.Sdk;
using Xunit;

namespace HQ.Platform.Identity.Tests.Sqlite
{
    public class SqliteRoleServiceTests : RoleServiceTests, IClassFixture<IdentitySqliteFixture>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public SqliteRoleServiceTests(IdentitySqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

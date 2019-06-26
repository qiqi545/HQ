using HQ.Test.Sdk;
using Xunit;

namespace HQ.Platform.Identity.Tests.Sqlite
{
    public class SqliteUserServiceTests : UserServiceTests, IClassFixture<IdentitySqliteFixture>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public SqliteUserServiceTests(IdentitySqliteFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}

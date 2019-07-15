using HQ.Integration.Sqlite.Sql;
using HQ.Test.Sdk.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Tests.Options.Sqlite
{
	public class SaveOptionsSqliteFixture : SqliteFixture
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddSqlite();
		}
	}
}

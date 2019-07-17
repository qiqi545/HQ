﻿using HQ.Integration.Tests.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Tests.Options.Sqlite
{
	public class SaveOptionsSqliteFixture : SqliteFixture
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			var configBuilder = new ConfigurationBuilder();
		}
	}
}
using System;
using HQ.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace HQ.Integration.SqlServer.Options
{
	public static class SqlServerConfigurationExtensions
	{
		public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, string path, IConfiguration configSeed = null)
		{
			return AddSqlServer(builder, provider: null, path: path, reloadOnChange: false, configSeed: configSeed);
		}

		public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, string path, bool reloadOnChange, IConfiguration configSeed = null)
		{
			return AddSqlServer(builder, provider: null, path: path, reloadOnChange: reloadOnChange, configSeed: configSeed);
		}

		public static IConfigurationBuilder AddSqlServer(this IConfigurationBuilder builder, IFileProvider provider, string path, bool reloadOnChange, IConfiguration configSeed, Action<SaveConfigurationOptions> configureAction = null)
		{
			throw new NotImplementedException();
			var saveConfig = new SaveConfigurationOptions();
			configureAction?.Invoke(saveConfig);
			return builder;
		}
	}
}

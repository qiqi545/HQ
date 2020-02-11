#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using HQ.Extensions.Options;
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Integration.DocumentDb.Sql.DbProvider;
using Microsoft.Extensions.Configuration;
using Constants = HQ.Common.Constants;

namespace HQ.Integration.DocumentDb.Options
{
	public static class Add
	{
		public static IConfigurationBuilder AddDocumentDb(this IConfigurationBuilder builder, string connectionString, IConfiguration configureOptions, bool reloadOnChange = false, IConfiguration configSeed = null)
		{
			return builder.AddDocumentDb(o => { DefaultDbOptions(connectionString, o); }, configureOptions.FastBind,
				reloadOnChange, configSeed);
		}

		public static IConfigurationBuilder AddDocumentDb(this IConfigurationBuilder builder, string connectionString, bool reloadOnChange = false, IConfiguration configSeed = null,
			Action<SaveConfigurationOptions> configureOptions = null)
		{
			return builder.AddDocumentDb(o => { DefaultDbOptions(connectionString, o); }, configureOptions,
				reloadOnChange, configSeed);
		}

		public static IConfigurationBuilder AddDocumentDb(this IConfigurationBuilder builder,
			Action<DocumentDbOptions> configureDatabase, Action<SaveConfigurationOptions> configureOptions = null,
			bool reloadOnChange = false, IConfiguration configSeed = null)
		{
			var dbConfig = new DocumentDbOptions();
			configureDatabase?.Invoke(dbConfig);

			var saveConfig = new SaveConfigurationOptions();
			configureOptions?.Invoke(saveConfig);

			var binders = new ICustomConfigurationBinder[] { new TypeDiscriminatorBinder() };

			var source = new DocumentConfigurationSource(dbConfig, saveConfig, configSeed)
			{
				ReloadOnChange = reloadOnChange
			};

			builder.Add(source);
			return builder;
		}

		private static void DefaultDbOptions(string connectionString, DocumentDbOptions o)
		{
			var connectionStringBuilder = new DocumentDbConnectionStringBuilder(connectionString);
			o.AccountKey ??= connectionStringBuilder.AccountKey;
			o.AccountEndpoint ??= connectionStringBuilder.AccountEndpoint;
			o.DatabaseId ??= connectionStringBuilder.Database;
			o.CollectionId ??= connectionStringBuilder.DefaultCollection ?? Constants.Options.DefaultCollection;

			o.SharedCollection = false;
			o.PartitionKeyPaths = connectionStringBuilder.PartitionKeyPaths ?? new[] {"/id"};
		}
	}
}
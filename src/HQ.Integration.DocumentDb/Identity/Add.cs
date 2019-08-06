
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
using System.Data;
using HQ.Common;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Dapper;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Metrics;
using HQ.Integration.DocumentDb.DbProvider;
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Integration.DocumentDb.Sql;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Stores.Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;

namespace HQ.Integration.DocumentDb.Identity
{
    public static class Add
    {
		public static IdentityBuilder AddDocumentDbIdentityStore(this IdentityBuilder identityBuilder, Action<DocumentDbOptions> configureAction, ConnectionScope scope = ConnectionScope.ByRequest)
		{
			return identityBuilder.AddDocumentDbIdentityStore<string, IdentityUserExtended, IdentityRoleExtended, IdentityTenant, IdentityApplication>(configureAction, scope);
		}

		public static IdentityBuilder AddDocumentDbIdentityStore<TUser, TRole, TTenant, TApplication>(
            this IdentityBuilder identityBuilder, 
            string connectionString, 
            ConnectionScope scope = ConnectionScope.ByRequest)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
            where TTenant : IdentityTenant<string>
            where TApplication : IdentityApplication<string>
        {
            return identityBuilder.AddDocumentDbIdentityStore<string, TUser, TRole, TTenant, TApplication>(connectionString, scope);
        }
		
        public static IdentityBuilder AddDocumentDbIdentityStore<TKey, TUser, TRole, TTenant, TApplication>(
            this IdentityBuilder identityBuilder,
            string connectionString,
            ConnectionScope scope = ConnectionScope.ByRequest)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
            where TApplication : IdentityApplication<TKey>
        {
	        return identityBuilder.AddDocumentDbIdentityStore<TKey, TUser, TRole, TTenant, TApplication>(o =>
	        {
		        var builder = new DocumentDbConnectionStringBuilder(connectionString);
		        o.AccountKey = o.AccountKey ?? builder.AccountKey;
		        o.AccountEndpoint = o.AccountEndpoint ?? builder.AccountEndpoint;
		        o.DatabaseId = o.DatabaseId ?? builder.Database;
		        o.CollectionId = o.CollectionId ?? builder.DefaultCollection ?? Constants.Identity.DefaultCollection;
	        }, scope);
        }

		public static IdentityBuilder AddDocumentDbIdentityStore<TKey, TUser, TRole, TTenant, TApplication>(
		   this IdentityBuilder identityBuilder,
		   Action<DocumentDbOptions> configureAction = null,
		   ConnectionScope scope = ConnectionScope.ByRequest)
		   where TKey : IEquatable<TKey>
		   where TUser : IdentityUserExtended<TKey>
		   where TRole : IdentityRoleExtended<TKey>
		   where TTenant : IdentityTenant<TKey>
		   where TApplication : IdentityApplication<TKey>
		{
			var services = identityBuilder.Services;

			const string slot = "Identity";

			if (configureAction != null)
				services.Configure(slot, configureAction);

			identityBuilder.Services.AddSingleton<ITypeRegistry, TypeRegistry>();

			var options = new DocumentDbOptions();
			configureAction?.Invoke(options);

			var builder = new DocumentDbConnectionStringBuilder(options);

			identityBuilder.AddSqlStores<DocumentDbConnectionFactory, TKey, TUser, TRole, TTenant, TApplication>(
				builder.ConnectionString, scope, OnCommand<TKey>(slot), OnConnection);

			var dialect = new DocumentDbDialect();
			SqlBuilder.Dialect = dialect;

			SimpleDataDescriptor.TableNameConvention = s =>
			{
				switch (s)
				{
					case nameof(IdentityRoleExtended):
						return nameof(IdentityRole);
					case nameof(IdentityUserExtended):
						return nameof(IdentityUser);
					default:
						return s;
				}
			};

			DescriptorColumnMapper.AddTypeMap<TUser>(StringComparer.Ordinal);
			DescriptorColumnMapper.AddTypeMap<TRole>(StringComparer.Ordinal);
			DescriptorColumnMapper.AddTypeMap<TTenant>(StringComparer.Ordinal);
			DescriptorColumnMapper.AddTypeMap<TApplication>(StringComparer.Ordinal);

			identityBuilder.Services.AddMetrics();
			identityBuilder.Services.TryAddSingleton<ISqlDialect>(dialect);
			identityBuilder.Services.TryAddSingleton(dialect);

			identityBuilder.Services.AddQueryableProvider<TUser>(slot);
			identityBuilder.Services.AddQueryableProvider<TRole>(slot);
			identityBuilder.Services.AddQueryableProvider<TTenant>(slot);
			identityBuilder.Services.AddQueryableProvider<TApplication>(slot);
			
			identityBuilder.Services.AddSingleton<IDataBatchOperation<DocumentDbBatchOptions>>(r => new DocumentDbBatchDataOperation(
				r.GetRequiredService<IServerTimestampService>(),
				r.GetRequiredService<IOptions<DocumentDbOptions>>(),
				r.GetRequiredService<IOptions<DocumentDbBatchOptions>>()));

			var serviceProvider = services.BuildServiceProvider();
			var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityOptionsExtended>>().Value;

			MigrateToLatest<TKey>(identityOptions, options);

			return identityBuilder;
		}

		private static void AddQueryableProvider<T>(this IServiceCollection services, string slot)
		{
			services.AddSingleton<IQueryableProvider<T>>(r => new DocumentDbQueryableProvider<T>(slot, r.GetRequiredService<DocumentDbConnectionFactory>(), r.GetService<IMetricsHost<DocumentClient>>(), r.GetRequiredService<IOptionsMonitor<DocumentDbOptions>>()));
		}

		private static void OnConnection(IDbConnection c, IServiceProvider r)
        {
            if (c is DocumentDbConnection)
            {

            }
        }

        private static Action<IDbCommand, Type, IServiceProvider> OnCommand<TKey>(string slot)
            where TKey : IEquatable<TKey>
        {
            return (c, t, r) =>
            {
                if (c is DocumentDbCommand command)
                {
                    var options = r.GetRequiredService<IOptionsMonitor<DocumentDbOptions>>();
                    var registry = r.GetRequiredService<ITypeRegistry>();

                    var descriptor = SimpleDataDescriptor.Create(t);
                    registry.TryRegister(t);

                    command.Id = descriptor.Id?.Property?.Name;
                    command.Type = t;
                    command.DocumentType = descriptor.Table;
                    command.Collection = options.Get(slot).CollectionId;
                }
            };
        }

		private static void MigrateToLatest<TKey>(IdentityOptionsExtended identityOptions, DocumentDbOptions dbOptions) where TKey : IEquatable<TKey>
		{
			var runner = new DocumentDbMigrationRunner(dbOptions);

			if (identityOptions.Stores.CreateIfNotExists)
			{
				runner.CreateDatabaseIfNotExistsAsync().Wait();
			}

			if (identityOptions.Stores.MigrateOnStartup)
			{
				runner.CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();
			}
		}
	}
}

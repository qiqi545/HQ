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
using System.Data.DocumentDb;
using System.Threading;
using HQ.Common;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.SessionManagement.DocumentDb;
using HQ.Data.Sql.Dapper;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.DocumentDb;
using HQ.Data.Sql.Queries;
using HQ.Extensions.Metrics;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Constants = HQ.Common.Constants;

namespace HQ.Platform.Identity.Stores.Sql.DocumentDb
{
    public static class Add
    {
        public static IdentityBuilder AddDocumentDbIdentityStore<TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
            where TTenant : IdentityTenant
        {
            return identityBuilder.AddDocumentDbIdentityStore<string, TUser, TRole, TTenant>(connectionString, scope,
                databaseConfig);
        }

        public static IdentityBuilder AddDocumentDbIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest,
            IConfiguration databaseConfig = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            var configureDatabase =
                databaseConfig != null ? databaseConfig.Bind : (Action<DocumentDbOptions>) null;

            return AddDocumentDbIdentityStore<TKey, TUser, TRole, TTenant>(identityBuilder, connectionString, scope,
                configureDatabase);
        }

        public static IdentityBuilder AddDocumentDbIdentityStore<TKey, TUser, TRole, TTenant>(
            this IdentityBuilder identityBuilder,
            string connectionString,
            ConnectionScope scope = ConnectionScope.ByRequest,
            Action<DocumentDbOptions> configureDocumentDb = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
            where TTenant : IdentityTenant<TKey>
        {
            var services = identityBuilder.Services;
            services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var serviceProvider = services.BuildServiceProvider();
            var builder = new DocumentDbConnectionStringBuilder(connectionString);

            void ConfigureAction(DocumentDbOptions o)
            {
                configureDocumentDb?.Invoke(o);
                o.DatabaseId = o.DatabaseId ?? builder.Database;
                o.CollectionId = o.CollectionId ?? builder.DefaultCollection ?? Constants.Identity.DefaultCollection;
            }

            identityBuilder.Services.Configure<DocumentDbOptions>(ConfigureAction);

            var dialect = new DocumentDbDialect();
            identityBuilder.AddSqlStores<DocumentDbConnectionFactory, TKey, TUser, TRole, TTenant>(connectionString,
                scope,
                OnCommand<TKey>(), OnConnection);

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

            services.AddMetrics();
            services.AddSingleton(dialect);
            services.AddSingleton<IQueryableProvider<TUser>, DocumentDbQueryableProvider<TUser>>();
            services.AddSingleton<IQueryableProvider<TRole>, DocumentDbQueryableProvider<TRole>>();
            services.AddSingleton<IQueryableProvider<TTenant>, DocumentDbQueryableProvider<TTenant>>();

            var options = new DocumentDbOptions();
            ConfigureAction(options);

            var identityOptions = serviceProvider.GetRequiredService<IOptions<IdentityOptionsExtended>>().Value;

            MigrateToLatest<TKey>(connectionString, identityOptions, options);

            return identityBuilder;
        }

        private static void OnConnection(IDbConnection c, IServiceProvider r)
        {
            if (c is DocumentDbConnection connection)
            {
            }
        }

        private static Action<IDbCommand, Type, IServiceProvider> OnCommand<TKey>()
            where TKey : IEquatable<TKey>
        {
            return (c, t, r) =>
            {
                if (c is DocumentDbCommand command)
                {
                    var options = r.GetRequiredService<IOptions<DocumentDbOptions>>();
                    var registry = r.GetRequiredService<ITypeRegistry>();

                    var descriptor = SimpleDataDescriptor.Create(t);
                    registry.TryRegister(t);

                    command.Id = descriptor.Id?.Property?.Name;
                    command.Type = t;
                    command.DocumentType = descriptor.Table;
                    command.Collection = options.Value.CollectionId;
                }
            };
        }

        private static void MigrateToLatest<TKey>(string connectionString, IdentityOptionsExtended identityOptions,
            DocumentDbOptions options) where TKey : IEquatable<TKey>
        {
            var runner = new MigrationRunner(connectionString, options);

            if (identityOptions.Stores.CreateIfNotExists)
            {
                runner.CreateDatabaseIfNotExistsAsync(CancellationToken.None).Wait();
            }

            if (identityOptions.Stores.MigrateOnStartup)
            {
                runner.MigrateUp(CancellationToken.None);
            }
        }
    }
}

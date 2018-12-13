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
using HQ.Cadence;
using HQ.Cohort.Configuration;
using HQ.Cohort.Models;
using HQ.Common.Models;
using HQ.Connect;
using HQ.Connect.DocumentDb;
using HQ.Connect.DocumentDb.Configuration;
using HQ.Lingo.Descriptor;
using HQ.Lingo.DocumentDb;
using HQ.Lingo.Queries;
using HQ.Rosetta.Queryable;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Cohort.Stores.Sql.DocumentDb
{
    public static class Add
    {
        public static IdentityBuilder AddDocumentDbIdentityStore<TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString, ConnectionScope scope = ConnectionScope.ByRequest)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
        {
            return identityBuilder.AddDocumentDbIdentityStore<string, TUser, TRole>(connectionString, null, scope);
        }

        public static IdentityBuilder AddDocumentDbIdentityStore<TUser, TRole>(this IdentityBuilder identityBuilder,
            string connectionString,
            IConfiguration documentDbConfig,
            ConnectionScope scope = ConnectionScope.ByRequest)
            where TUser : IdentityUserExtended<string>
            where TRole : IdentityRoleExtended<string>
        {
            return identityBuilder.AddDocumentDbIdentityStore<string, TUser, TRole>(connectionString, documentDbConfig, scope);
        }

        public static IdentityBuilder AddDocumentDbIdentityStore<TKey, TUser, TRole>(
            this IdentityBuilder identityBuilder,
            string connectionString,
            IConfiguration documentDbConfig,
            ConnectionScope scope)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
        {
            if (documentDbConfig != null) identityBuilder.Services.Configure<DocumentDbOptions>(documentDbConfig);

            var configureDocumentDb =
                documentDbConfig != null ? documentDbConfig.Bind : (Action<DocumentDbOptions>) null;

            return AddDocumentDbIdentityStore<TKey, TUser, TRole>(identityBuilder, connectionString, scope, null,
                configureDocumentDb);
        }

        public static IdentityBuilder AddDocumentDbIdentityStore<TKey, TUser, TRole>(
            this IdentityBuilder identityBuilder,
            string connectionString,
            ConnectionScope scope = ConnectionScope.ByRequest,
            Action<IdentityOptionsExtended> configureIdentity = null,
            Action<DocumentDbOptions> configureDocumentDb = null)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUserExtended<TKey>
            where TRole : IdentityRoleExtended<TKey>
        {
            identityBuilder.Services.AddSingleton<ITypeRegistry, TypeRegistry>();

            var serviceProvider = identityBuilder.Services.BuildServiceProvider();

            var identityOptions = serviceProvider.GetService<IOptions<IdentityOptionsExtended>>()?.Value ??
                                  new IdentityOptionsExtended();
            configureIdentity?.Invoke(identityOptions);

            var documentDbOptions = new DocumentDbOptions();
            configureDocumentDb?.Invoke(documentDbOptions);

            var dialect = new DocumentDbDialect();
            var builder = new DocumentDbConnectionStringBuilder(connectionString);

            documentDbOptions.CollectionId = documentDbOptions.CollectionId ??
                                             builder.DefaultCollection ?? Common.Constants.Identity.DefaultCollection;

            identityBuilder.AddSqlStores<DocumentDbConnectionFactory, TKey, TUser, TRole>(connectionString, scope,
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

            identityBuilder.Services.AddMetrics();
            identityBuilder.Services.AddSingleton(dialect);
            identityBuilder.Services.AddSingleton<IQueryableProvider<TUser>, DocumentDbQueryableProvider<TUser>>();
            identityBuilder.Services.AddSingleton<IQueryableProvider<TRole>, DocumentDbQueryableProvider<TRole>>();

            lock (identityBuilder)
            {
                MigrateToLatest<TKey>(connectionString, identityOptions, documentDbOptions);
            }

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
                    registry.RegisterIfNotRegistered(t);

                    command.Id = descriptor.Id?.Property?.Name;
                    command.Type = t;
                    command.DocumentType = descriptor.Table;
                    command.Collection = options.Value.CollectionId;
                }
            };
        }
        private static void MigrateToLatest<TKey>(string connectionString, IdentityOptionsExtended identityOptions, DocumentDbOptions options) where TKey : IEquatable<TKey>
        {
            var runner = new MigrationRunner(connectionString, options);

            if (identityOptions.Stores.CreateIfNotExists)
                runner.CreateDatabaseIfNotExistsAsync(CancellationToken.None).Wait();

            if (identityOptions.Stores.MigrateOnStartup)
                runner.MigrateUp(CancellationToken.None);
        }
    }
}

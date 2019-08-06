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
using System.Reflection;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.SessionManagement;
using HQ.Extensions.Deployment;
using HQ.Extensions.Logging;
using HQ.Integration.DocumentDb.Identity;
using HQ.Integration.DocumentDb.Sql;
using HQ.Integration.Sqlite.Identity;
using HQ.Integration.Sqlite.Runtime;
using HQ.Integration.Sqlite.Sql.Configuration;
using HQ.Integration.SqlServer.Identity;
using HQ.Integration.SqlServer.Sql.Configuration;
using HQ.Platform.Api;
using HQ.Platform.Api.Functions.AspNetCore.Mvc;
using HQ.Platform.Api.Runtime.GraphQl;
using HQ.Platform.Api.Runtime.Rest;
using HQ.Platform.Identity;
using HQ.Platform.Identity.AspNetCore.Mvc;
using HQ.Platform.Identity.Models;
using HQ.Platform.Node.UI.Pages;
using HQ.Platform.Operations;
using HQ.Platform.Security.AspNetCore;
using HQ.UI;
using HQ.UI.Web;
using HQ.UI.Web.SemanticUi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Platform.Node
{
	public interface IBackend { }
	public sealed class DocumentDb : IBackend { }
	public sealed class SqlServer : IBackend { }
	public sealed class Sqlite : IBackend { }
	
	public static class Add
    {
        public static IServiceCollection AddHq<TBackend>(
            this IServiceCollection services,
            IHostingEnvironment env,
            IConfiguration config,
            ISafeLogger logger,
            params ICloudOptions[] clouds) where TBackend : IBackend
        {
            var subject = Assembly.GetCallingAssembly();

            if (!(config is IConfigurationRoot configRoot))
            {
                throw new ArgumentException("HQ requires access to the root configuration.", nameof(config));
            }

            //
            // Core Configuration:
            var hq = configRoot.GetSection("HQ");
            var security = hq.GetSection("Security");
            var identity = hq.GetSection("Identity");
            var identityApi = identity.GetSection("Api");
            var api = hq.GetSection("Api");
            var versioning = api.GetSection("Versioning");
            var multiTenants = api.GetSection("MultiTenancy");
            var ops = hq.GetSection("Ops");
            var tasks = hq.GetSection("BackgroundTasks");
			var configApi = hq.GetSection("Configuration");
			var meta = hq.GetSection("Meta");
			var runtime = hq.GetSection("Runtime");

            services.TryAddSingleton(configRoot);
            services.TryAddSingleton(services);

			//
			// Core Services:
			services.AddLocalTimestamps();
			services.AddSafeLogging();
			services.AddSecurityPolicies(security, logger);
            services.AddOperationsApi(env, ops);
            services.AddPlatformApi(api);
            services.AddMultiTenancy<IdentityTenant, IdentityApplication>(multiTenants)
                .AddIdentityTenantContextStore<IdentityTenant>()
                .AddIdentityApplicationContextStore<IdentityApplication>();
            services.AddVersioning(versioning);
            services
	            .AddBackgroundTasksApi(tasks)
	            .AddConfigurationApi(configRoot, configApi)
				.AddIdentityApi(identityApi)
                .AddMetaApi(meta)
	            .AddRuntimeApi(runtime)
                ;

            //
            // Cloud:
            services.AddCloudServices(logger, clouds);

            //
            // Database:
            var connectionString = configRoot.GetConnectionString("DefaultConnection");
            var dbOptions = configRoot.GetSection("DbOptions");
            services.AddBackendServices<TBackend>(env, connectionString, dbOptions, hq, logger, subject);

            return services;
        }

        public static IServiceCollection AddBackendServices<TBackend>(this IServiceCollection services,
            IHostingEnvironment env, string connectionString, IConfiguration dbConfig, IConfiguration appConfig,
            ISafeLogger logger, Assembly subjectAssembly, string rootPath = "/api") where TBackend : IBackend
		{
            var identity = services
                .AddIdentityExtended(appConfig.GetSection("Identity"));

            switch (typeof(TBackend).Name)
            {
                case nameof(DocumentDb):
                    identity
                        .AddDocumentDbIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
                            IdentityApplication>(connectionString);
                    break;
                case nameof(SqlServer):
                    identity
                        .AddSqlServerIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
                            IdentityApplication>(
                            connectionString, ConnectionScope.ByRequest,
                            dbConfig);
                    break;
                case nameof(Sqlite):
                    identity
                        .AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
                            IdentityApplication>(
                            connectionString, ConnectionScope.ByRequest,
                            dbConfig);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(TBackend), typeof(TBackend), null);
            }

			// FIXME: IRuntimeBuilder?
			services.AddSqliteRuntime(connectionString, ConnectionScope.ByRequest, dbConfig);
			services.AddRestRuntime();
            services.AddGraphQlRuntime();

			UiConfig.Settings = settings =>
            {
                settings.DefaultPageTitle = Assembly.GetCallingAssembly().GetName().Name;
                settings.ComponentAssemblies = new[]
                {
                    typeof(UiComponent).Assembly,   // HQ.UI
                    typeof(HtmlSystem).Assembly,    // HQ.UI.Web
                    typeof(SemanticUi).Assembly,    // HQ.Web.Semantic.Ui
                    typeof(Dashboard).Assembly,     // HQ.Platform.Node
                    Assembly.GetEntryAssembly()     // App
                };
            };

            services.AddUi(env, typeof(SemanticUi).Assembly);

            ScanForGeneratedObjects<TBackend>(services, appConfig, logger, rootPath, subjectAssembly);

            return services;
        }

        private static void ScanForGeneratedObjects<TBackend>(IServiceCollection services,
            IConfiguration appConfig,
            ISafeLogger logger, string rootPath, Assembly assembly) where TBackend : IBackend
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!type.IsAbstract || !type.IsSealed)
                {
                    continue;
                }

                if (type.Name != "ServiceCollectionExtensions")
                {
                    continue;
                }

                var method = type.GetMethod("AddGenerated",
                    new[] {typeof(IServiceCollection), typeof(IConfiguration), typeof(string)});
                if (method == null)
                {
                    continue;
                }

                Type batchOptionsType;
				switch (typeof(TBackend).Name)
				{
					case nameof(DocumentDb):
						batchOptionsType = typeof(DocumentDbBatchOptions);
						break;
					case nameof(SqlServer):
						batchOptionsType = typeof(SqlServerBatchOptions);
						break;
					case nameof(Sqlite):
						batchOptionsType = typeof(SqliteBatchOptions);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

                logger.Info(() => "Found generated objects API in {AssemblyName}", assembly.GetName().Name);
                method.MakeGenericMethod(batchOptionsType)
                    .Invoke(null, new object[] {services, appConfig.GetSection("Security"), rootPath});
            }
        }
    }
}

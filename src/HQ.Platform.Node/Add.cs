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
using HQ.Integration.DocumentDb.Runtime;
using HQ.Integration.DocumentDb.Sql;
using HQ.Integration.Sqlite.Identity;
using HQ.Integration.Sqlite.Runtime;
using HQ.Integration.Sqlite.Sql.Configuration;
using HQ.Integration.SqlServer.Identity;
using HQ.Integration.SqlServer.Runtime;
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
	    public static IServiceCollection AddHq(
		    this IServiceCollection services,
		    IHostingEnvironment env,
		    IConfiguration config,
		    ISafeLogger logger,
		    params ICloudOptions[] clouds)
	    {
		    var subject = Assembly.GetCallingAssembly();

		    if (!(config is IConfigurationRoot configRoot))
		    {
			    throw new ArgumentException("HQ requires access to the root configuration.", nameof(config));
		    }

		    services.TryAddSingleton(configRoot);
		    services.TryAddSingleton(services);

		    var hq = configRoot.GetSection("HQ");

		    //
		    // Core Services:
		    services.AddLocalTimestamps();
		    services.AddSafeLogging();
		    services.AddSecurityPolicies(hq.GetSection("Security"), logger);
		    services.AddOperationsApi(env, hq.GetSection("Ops"));
		    services.AddPlatformApi(hq.GetSection("Api"));
		    services.AddMultiTenancy<IdentityTenant, IdentityApplication>(hq.GetSection("MultiTenancy"))
			    .AddIdentityTenantContextStore<IdentityTenant>()
			    .AddIdentityApplicationContextStore<IdentityApplication>();
		    services.AddVersioning(hq.GetSection("Versioning"));
		    services
			    .AddBackgroundTasksApi(hq.GetSection("BackgroundTasks"))
			    .AddConfigurationApi(configRoot, hq.GetSection("Configuration"))
			    .AddIdentityApi(hq.GetSection("IdentityApi"))
			    .AddMetaApi(hq.GetSection("Meta"))
			    .AddRuntimeApi(hq.GetSection("Runtime"))
			    ;
		    var identity = services
			    .AddIdentityExtended(hq.GetSection("Identity"));

		    //
		    // Cloud:
		    services.AddCloudServices(logger, clouds);

			//
			// Backend Services:
			var backend = configRoot.GetSection("Backend");
			var connectionString = backend.GetConnectionString("DefaultConnection");
		    var dbConfig = backend.GetSection("DbOptions");
		    var backendType = backend["Type"];
		    switch (backendType)
		    {
			    case nameof(DocumentDb):
				    identity
					    .AddDocumentDbIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
						    IdentityApplication>(connectionString);
				    services.AddDocumentDbRuntime(connectionString, ConnectionScope.ByRequest);
				    break;
			    case nameof(SqlServer):
				    identity
					    .AddSqlServerIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
						    IdentityApplication>(
						    connectionString, ConnectionScope.ByRequest,
						    dbConfig);
				    services.AddSqlServerRuntime(connectionString, ConnectionScope.ByRequest, dbConfig);
				    break;
			    case nameof(Sqlite):
				    identity
					    .AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
						    IdentityApplication>(
						    connectionString, ConnectionScope.ByRequest,
						    dbConfig);
				    services.AddSqliteRuntime(connectionString, ConnectionScope.ByRequest, dbConfig);
				    break;
			    default:
				    throw new ArgumentOutOfRangeException(backendType, typeof(string), null);
		    }

		    services.AddRestRuntime();
		    services.AddGraphQlRuntime();

		    UiConfig.Settings = settings =>
		    {
			    settings.DefaultPageTitle = Assembly.GetCallingAssembly().GetName().Name;
			    settings.ComponentAssemblies = new[]
			    {
				    typeof(UiComponent).Assembly, // HQ.UI
				    typeof(HtmlSystem).Assembly, // HQ.UI.Web
				    typeof(SemanticUi).Assembly, // HQ.Web.Semantic.Ui
				    typeof(Dashboard).Assembly, // HQ.Platform.Node
				    Assembly.GetEntryAssembly() // App
			    };
		    };

		    services.AddUi(env, typeof(SemanticUi).Assembly);

		    services.ScanForGeneratedObjects(backendType, hq.GetSection("Security"), logger, "/api", subject);

		    return services;
	    }

	    private static void ScanForGeneratedObjects(this IServiceCollection services, string backendType, IConfiguration security, ISafeLogger logger, string rootPath, Assembly assembly)
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
				switch (backendType)
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
                    .Invoke(null, new object[] {services, security, rootPath});
            }
        }
    }
}

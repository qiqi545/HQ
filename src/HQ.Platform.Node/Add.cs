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

using Microsoft.AspNetCore.Hosting;
using System;
using System.Reflection;
using ActiveLogging;
using ActiveOptions;
using ActiveOptions.Api;
using ActiveRoutes.Meta;
using ActiveScheduler;
using ActiveTenant;
using ActiveTenant.Internal;
using HQ.Common;
using HQ.Data.Contracts.Schema;
using HQ.Data.SessionManagement;
using HQ.Extensions.Deployment;
using HQ.Extensions.Notifications;
using HQ.Integration.Azure;
using HQ.Integration.DocumentDb.Identity;
using HQ.Integration.DocumentDb.Runtime;
using HQ.Integration.DocumentDb.Scheduling;
using HQ.Integration.DocumentDb.Schema;
using HQ.Integration.DocumentDb.Sql;
using HQ.Integration.Sqlite.Identity;
using HQ.Integration.Sqlite.Runtime;
using HQ.Integration.Sqlite.Scheduling;
using HQ.Integration.Sqlite.Schema;
using HQ.Integration.Sqlite.Sql.Configuration;
using HQ.Integration.SqlServer.Identity;
using HQ.Integration.SqlServer.Runtime;
using HQ.Integration.SqlServer.Scheduling;
using HQ.Integration.SqlServer.Schema;
using HQ.Integration.SqlServer.Sql.Configuration;
using HQ.Platform.Api;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Correlation;
using HQ.Platform.Api.Functions.AspNetCore.Mvc;
using HQ.Platform.Api.Runtime.GraphQl;
using HQ.Platform.Api.Runtime.Rest;
using HQ.Platform.Identity;
using HQ.Platform.Identity.Models;
using HQ.Platform.Identity.Mvc;
using HQ.Platform.Operations;
using HQ.Platform.Security.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Platform.Node
{
	#region Sentinels

	#endregion

	public static class Add
	{
		public static IServiceCollection AddHq(this IServiceCollection services, IWebHostEnvironment env,
			IConfiguration config, ISafeLogger logger)
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
			services.AddTypeResolver();
			services.AddLocalTimestamps();
			services.AddSafeLogging();
			services.AddValidOptions();
			services.AddTraceContext();

			//
			// Platform Services:
			services.AddSecurityPolicies(hq.GetSection("Security"), hq.GetSection("SuperUser"), logger);
			services.AddOperationsApi(hq.GetSection("Ops"));
			services.AddPlatformApi(hq.GetSection("Api"));
			services.AddMultiTenancy<IdentityTenant, IdentityApplication>(hq.GetSection("MultiTenancy"))
				.AddIdentityTenantContextStore<IdentityTenant>()
				.AddIdentityApplicationContextStore<IdentityApplication>();
			services.AddVersioning(hq.GetSection("Versioning"));
			services
				.AddBackgroundTasksApi(hq.GetSection("BackgroundTasks"))
				.AddConfigurationApi(configRoot, hq.GetSection("Configuration"))
				.AddIdentityApi(hq.GetSection("IdentityApi"))
				.AddSchemaApi(hq.GetSection("Schema"))
				.AddMetaApi(hq.GetSection("Meta"));

			var tasksBuilder = services.AddBackgroundTasks(hq.GetSection("BackgroundTasks"));
			var identityBuilder = services.AddIdentityExtended(hq.GetSection("Identity"));
			var runtimeBuilder = services.AddRuntimeApi(hq.GetSection("Runtime"));
			var schemaBuilder = services.AddSchemaDiscovery(hq.GetSection("Schema"));

			//
			// Cloud:
			var cloud = configRoot.GetSection("Cloud");
			switch (cloud["Provider"])
			{
				case nameof(Azure):
				{
					var options = new AzureOptions();
					cloud.FastBind(options);
					services.AddCloudServices(logger, options);
					break;
				}
			}

			//
			// Backend Services:
			var backend = configRoot.GetSection("Backend");
			var dbConfig = backend.GetSection("DbOptions");
			if (dbConfig?.Value == null)
				dbConfig = null;

			var backendType = backend["Type"];
			if (string.IsNullOrWhiteSpace(backendType))
				logger.Warn(() => "No backend type found!");
			else
				logger.Info(() => "Installing {BackendType} back-end services.", backendType);

			switch (backendType)
			{
				case nameof(DocumentDb):
					tasksBuilder.AddDocumentDbBackgroundTaskStore(backend.GetConnectionString("Tasks"));
					identityBuilder
						.AddDocumentDbIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
							IdentityApplication>(backend.GetConnectionString("Identity"));
					runtimeBuilder.AddDocumentDbRuntimeStores(backend.GetConnectionString("Runtime"),
						ConnectionScope.ByRequest, dbConfig);
					schemaBuilder.AddDocumentDbSchemaStores(backend.GetConnectionString("Schema"));
					break;
				case nameof(SqlServer):
					tasksBuilder.AddSqlServerBackgroundTasksStore(backend.GetConnectionString("Tasks"),
						ConnectionScope.ByRequest, dbConfig);
					identityBuilder
						.AddSqlServerIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
							IdentityApplication>(backend.GetConnectionString("Identity"), ConnectionScope.ByRequest,
							dbConfig);
					runtimeBuilder.AddSqlServerRuntime(backend.GetConnectionString("Runtime"),
						ConnectionScope.ByRequest, dbConfig);
					schemaBuilder.AddSqlServerSchemaStores();
					break;
				case nameof(Sqlite):
					tasksBuilder.AddSqliteBackgroundTasksStore(backend.GetConnectionString("Tasks"),
						ConnectionScope.ByRequest, dbConfig);
					identityBuilder
						.AddSqliteIdentityStore<IdentityUserExtended, IdentityRoleExtended, IdentityTenant,
							IdentityApplication>(backend.GetConnectionString("Identity"), ConnectionScope.ByRequest,
							dbConfig);
					runtimeBuilder.AddSqliteRuntime(backend.GetConnectionString("Runtime"), ConnectionScope.ByRequest,
						dbConfig);
					schemaBuilder.AddSqliteSchemaStores();
					break;
				default:
					throw new ArgumentOutOfRangeException(backendType, typeof(string), null);
			}

			//
			// Runtime Services:
			{
				var runtimeOptions = new RuntimeOptions();
				hq.GetSection("Runtime").FastBind(runtimeOptions);

				if (runtimeOptions.EnableRest)
				{
					services.AddRestRuntime();
					logger.Info(() => "REST is enabled.");
				}

				if (runtimeOptions.EnableGraphQl)
				{
					services.AddGraphQlRuntime();
					logger.Info(() => "GraphQL is enabled.");
				}
			}

			//
			// Notification Services:
			services.AddEmailNotifications(hq.GetSection("Email"));

			//
			// Media Services:

			//
			// Custom Objects:
			services.ScanForGeneratedObjects(backendType, hq.GetSection("Security"), logger, "/api", subject);

			return services;
		}

		private static void ScanForGeneratedObjects(this IServiceCollection services, string backendType,
			IConfiguration security, ISafeLogger logger, string rootPath, Assembly assembly)
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
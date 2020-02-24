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
using System.Linq;
using ActiveLogging;
using HQ.Extensions.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HQ.Extensions.Deployment
{
	public static class CloudExtensions
	{
		public static void ConfigureCloudLogging(this ILoggingBuilder builder, params ICloudOptions[] clouds)
		{
			foreach (var cloud in clouds)
			{
				var method = typeof(CloudExtensions).GetMethod(nameof(AddCloudLogging))
					?.MakeGenericMethod(cloud.GetType());
				method?.Invoke(null, new object[] {builder, cloud});
			}
		}

		public static IServiceCollection AddCloudServices(this IServiceCollection services, ISafeLogger logger,
			params ICloudOptions[] clouds)
		{
			foreach (var cloud in clouds)
			{
				var method = typeof(CloudExtensions).GetMethod(nameof(AddCloudTelemetry))
					?.MakeGenericMethod(cloud.GetType());
				method?.Invoke(null, new object[] {services, logger, cloud});
			}

			services.AddMetrics(builder =>
			{
				foreach (var cloud in clouds)
				{
					var method = typeof(CloudExtensions).GetMethod(nameof(AddCloudMetricsPublisher))
						?.MakeGenericMethod(cloud.GetType());
					method?.Invoke(null, new object[] {builder, logger, cloud});
				}
			});
			return services;
		}

		public static ILoggingBuilder AddCloudLogging<T>(this ILoggingBuilder builder, ICloudOptions options)
			where T : ICloudOptions
		{
			foreach (var module in ScanForTypesImplementing<ICloudLogger<T>>())
			{
				var method = typeof(ICloudLogger<T>).GetMethod(nameof(ICloudLogger<T>.AddCloudLogger));
				method?.Invoke(module, new object[] {builder, options});
			}

			return builder;
		}

		public static IServiceCollection AddCloudTelemetry<T>(this IServiceCollection services, ISafeLogger logger,
			ICloudOptions options)
			where T : ICloudOptions
		{
			foreach (var module in ScanForTypesImplementing<ICloudTelemetry<T>>())
			{
				var method = typeof(ICloudTelemetry<T>).GetMethod(nameof(ICloudTelemetry<T>.AddCloudTelemetry));
				method?.Invoke(module, new object[] {services, logger, options});
			}

			return services;
		}

		public static IMetricsBuilder AddCloudMetricsPublisher<T>(this IMetricsBuilder builder, ISafeLogger logger,
			ICloudOptions options)
			where T : ICloudOptions
		{
			foreach (var module in ScanForTypesImplementing<ICloudMetricsPublisher<T>>())
			{
				var method =
					typeof(ICloudMetricsPublisher<T>).GetMethod(nameof(ICloudMetricsPublisher<T>
						.AddCloudMetricsPublisher));
				method?.Invoke(module, new object[] {builder, logger, options});
			}

			return builder;
		}

		private static IEnumerable<T> ScanForTypesImplementing<T>() where T : class
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToArray();

			foreach (var assembly in assemblies)
			{
				if (assembly.IsDynamic)
				{
					continue;
				}

				foreach (var type in assembly.GetExportedTypes())
				{
					if (type.IsAbstract || type.IsInterface)
					{
						continue;
					}

					if (!typeof(T).IsAssignableFrom(type))
					{
						continue;
					}

					var instance = (T) Activator.CreateInstance(type);
					yield return instance;
				}
			}
		}
	}
}
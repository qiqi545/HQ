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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HQ.Platform.Api.Operations
{
	internal static class OperationsHealthChecks
	{
		public class OptionsHealth : IHealthCheck
		{
			private readonly IServiceProvider _serviceProvider;

			public OptionsHealth(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

			public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
				CancellationToken cancellationToken = new CancellationToken())
			{
				string description;
				HealthStatus status;
				Exception exception = null;
				IReadOnlyDictionary<string, object> data = null;

				try
				{
					var report = OperationsMethods.OptionsReport(_serviceProvider);

					status = report.HasErrors ? HealthStatus.Unhealthy : HealthStatus.Healthy;
					data = status != HealthStatus.Healthy
						? report.Options.Where(x => x.HasErrors).ToDictionary(k => k.Scope, v => (object) v.Values)
						: null;

					switch (status)
					{
						case HealthStatus.Unhealthy:
							description =
								"The options configuration for this application has one or more binding errors, hiding runtime exceptions.";
							break;
						case HealthStatus.Healthy:
							description = "The options configuration for this application is binding correctly.";
							break;
						case HealthStatus.Degraded:
							throw new NotImplementedException();
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception e)
				{
					status = HealthStatus.Unhealthy;
					description = "The options configuration health check faulted.";
					exception = e;
				}

				var result = new HealthCheckResult(status, description, exception, data);

				return Task.FromResult(result);
			}
		}

		public class ServicesHealth : IHealthCheck
		{
			private readonly IServiceProvider _serviceProvider;

			public ServicesHealth(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

			public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
				CancellationToken cancellationToken = new CancellationToken())
			{
				string description;
				HealthStatus status;
				Exception exception = null;
				IReadOnlyDictionary<string, object> data = null;

				try
				{
					var report = OperationsMethods.ServicesReport(_serviceProvider);

					status = report.MissingRegistrations.Count > 0 ? HealthStatus.Degraded : HealthStatus.Healthy;
					data = status == HealthStatus.Healthy
						? null
						: report.MissingRegistrations.ToDictionary(k => k,
							v => (object) report.Services.FirstOrDefault(x => x.ServiceType == v));

					switch (status)
					{
						case HealthStatus.Unhealthy:
							description =
								"The DI container for this application has a missing registration, hiding a runtime exception.";
							break;
						case HealthStatus.Healthy:
							description = "The DI container is correctly configured for this application.";
							break;
						case HealthStatus.Degraded:
							description =
								"The DI container for this application has a missing registration, hiding a runtime exception.";
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception e)
				{
					status = HealthStatus.Unhealthy;
					description = "The DI container health check faulted.";
					exception = e;
				}

				var result = new HealthCheckResult(status, description, exception, data);

				return Task.FromResult(result);
			}
		}
	}
}
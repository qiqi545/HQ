// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.MissionControl.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.MissionControl
{
	public static class Add
	{
		public static IServiceCollection AddDevOpsApi(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<DevOpsApiOptions>(config);
			services.AddSingleton(config);
			return services;
		}
	}
}
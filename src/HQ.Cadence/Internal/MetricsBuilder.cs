// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cadence.Internal
{
	internal class MetricsBuilder : IMetricsBuilder
	{
		public MetricsBuilder(IServiceCollection services)
		{
			Services = services;
		}

		public IServiceCollection Services { get; }
	}
}
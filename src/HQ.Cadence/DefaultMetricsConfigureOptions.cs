// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.Extensions.Options;

namespace HQ.Cadence
{
	internal class DefaultMetricsConfigureOptions : ConfigureOptions<MetricsOptions>
	{
		public DefaultMetricsConfigureOptions() : base(DefaultOptionsBuilder())
		{
		}

		private static Action<MetricsOptions> DefaultOptionsBuilder()
		{
			return options =>
			{
				options.Path = "metrics";
				options.Timeout = TimeSpan.FromSeconds(5);
			};
		}
	}
}
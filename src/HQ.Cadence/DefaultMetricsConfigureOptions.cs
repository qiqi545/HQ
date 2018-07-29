// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Microsoft.Extensions.Options;

namespace HQ.Cadence
{
	internal class DefaultMetricsConfigureOptions : ConfigureOptions<MetricsOptions>
	{
		public DefaultMetricsConfigureOptions() : base(DefaultOptionsBuilder()) { }

		static Action<MetricsOptions> DefaultOptionsBuilder()
		{
			return options => { };
		}
	}
}
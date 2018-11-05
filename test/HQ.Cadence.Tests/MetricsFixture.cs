// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Cadence.Tests
{
	public class MetricsFixture : IDisposable
	{
		public MetricsHost Metrics { get; }

		public MetricsFixture()
		{
			Metrics = new MetricsHost(new InMemoryMetricsStore());
		}

		public void Dispose()
		{
			Metrics.Clear();
		}
	}
}
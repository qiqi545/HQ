// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading.Tasks;

namespace HQ.Cadence
{
	public interface IMetricsReporter : IDisposable
	{
		Task Start();
	}
}
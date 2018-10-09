// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cadence
{
	public interface IMetricsBuilder
	{
		/// <summary>
		///     Gets the <see cref="IServiceCollection" /> where metrics services are configured.
		/// </summary>
		IServiceCollection Services { get; }
	}
}
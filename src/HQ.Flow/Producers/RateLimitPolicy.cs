// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Producers
{
	/// <summary>
	///     A rate limit policy, for use with <see cref="BackgroundThreadProducer{T}" />
	/// </summary>
	public class RateLimitPolicy
	{
		public bool Enabled { get; set; }
		public int Occurrences { get; set; }
		public TimeSpan TimeUnit { get; set; }
	}
}
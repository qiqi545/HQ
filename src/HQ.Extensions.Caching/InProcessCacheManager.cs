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
using System.Reflection;
using HQ.Common;
using HQ.Extensions.Caching.Configuration;
using HQ.Extensions.Caching.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Caching
{
	public abstract class InProcessCacheManager : ICacheManager
	{
		private readonly MemoryCacheOptions _memoryCacheOptions;
		protected readonly IMemoryCache Cache;
		protected readonly IOptions<CacheOptions> CacheOptions;

		protected InProcessCacheManager(IOptions<CacheOptions> cacheOptions, IServerTimestampService timestamps)
		{
			_memoryCacheOptions = new MemoryCacheOptions
			{
				CompactionPercentage = 0.05,
				ExpirationScanFrequency = TimeSpan.FromMinutes(1.0),
				SizeLimit = cacheOptions.Value.MaxSizeBytes,
				Clock = new ServerTimestampServiceSystemClock(timestamps)
			};
			Cache = new MemoryCache(_memoryCacheOptions);
			CacheOptions = cacheOptions;
		}

		#region ICacheManager

		public int Count
		{
			get
			{
				if (!(Cache is MemoryCache memory))
					return 0;
				var getCount = typeof(MemoryCache).GetProperty("Count", BindingFlags.Instance | BindingFlags.Public);
				return (int) (getCount?.GetValue(memory) ?? 0);
			}
		}

		public long SizeBytes
		{
			get
			{
				if (!(Cache is MemoryCache memory))
					return 0L;
				var getSize = typeof(MemoryCache).GetProperty("Size", BindingFlags.Instance | BindingFlags.NonPublic);
				return (long) (getSize?.GetValue(memory) ?? 0L);
			}
			set => SizeLimitBytes = value;
		}

		public long? SizeLimitBytes
		{
			get => _memoryCacheOptions.SizeLimit;
			set => _memoryCacheOptions.SizeLimit = value;
		}

		public void Clear()
		{
			if (Cache is MemoryCache memory)
				memory.Compact(1);
		}

		#endregion
	}
}
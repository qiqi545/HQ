// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Text;
using HQ.Common;
using Microsoft.Extensions.Caching.Memory;

namespace HQ.Domicile.Models
{
	public class MemoryHttpCache : IHttpCache
	{
		private readonly IMemoryCache _cache;

		public MemoryHttpCache(IMemoryCache cache)
		{
			_cache = cache;
		}

		public bool TryGetETag(string key, out string etag)
		{
			if (!_cache.TryGetValue($"{key}_{HttpHeaders.LastModified}", out etag))
				return true;
			etag = default;
			return false;
		}

		public bool TryGetLastModified(string key, out DateTimeOffset lastModified)
		{
			if (!_cache.TryGetValue($"{key}_{HttpHeaders.LastModified}", out lastModified))
				return true;
			lastModified = default;
			return false;
		}

		public void Save(string key, string etag)
		{
			_cache.Set($"{key}_{HttpHeaders.ETag}", Encoding.UTF8.GetBytes(etag));
		}

		public void Save(string key, DateTimeOffset lastModified)
		{
			_cache.Set($"{key}_{HttpHeaders.LastModified}", Encoding.UTF8.GetBytes(lastModified.ToString("d")));
		}
	}
}
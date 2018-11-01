// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Text;
using HQ.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace HQ.Domicile.Models
{
	public class DistributedHttpCache : IHttpCache
	{
		private readonly IDistributedCache _cache;

		public DistributedHttpCache(IDistributedCache cache)
		{
			_cache = cache;
		}

		public bool TryGetETag(string key, out string etag)
		{
			var buffer = _cache.Get($"{key}_{HttpHeaders.ETag}");
			if (buffer != null)
			{
				etag = Encoding.UTF8.GetString(buffer);
				return true;
			}

			etag = default;
			return false;
		}

		public bool TryGetLastModified(string key, out DateTimeOffset lastModified)
		{
			var buffer = _cache.Get($"{key}_{HttpHeaders.LastModified}");
			if (buffer != null)
			{
				var input = Encoding.UTF8.GetString(buffer);
				if (DateTimeOffset.TryParse(input, out lastModified))
					return true;
			}

			lastModified = default;
			return false;
		}

		public void Save(string key, string etag)
		{
			_cache.Set($"{key}_{HttpHeaders.ETag}", Encoding.UTF8.GetBytes(etag));
		}

		public void Save(string key, DateTimeOffset lastModified)
		{
			_cache.Set($"{key}_{HttpHeaders.LastModified}", Encoding.UTF8.GetBytes(lastModified.ToString("R")));
		}
	}
}
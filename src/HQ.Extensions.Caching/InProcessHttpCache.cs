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
using System.Text;
using HQ.Common;
using HQ.Extensions.Caching.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Caching
{
	public class InProcessHttpCache : InProcessCacheManager, IHttpCache
	{
		public InProcessHttpCache(IOptions<CacheOptions> cacheOptions, IServerTimestampService timestamps) : base(
			cacheOptions, timestamps)
		{
		}

		public bool TryGetETag(string key, out string etag)
		{
			if (!Cache.TryGetValue($"{key}_{Constants.HttpHeaders.LastModified}", out etag))
			{
				return true;
			}

			etag = default;
			return false;
		}

		public bool TryGetLastModified(string key, out DateTimeOffset lastModified)
		{
			if (!Cache.TryGetValue($"{key}_{Constants.HttpHeaders.LastModified}", out lastModified))
			{
				return true;
			}

			lastModified = default;
			return false;
		}

		public void Save(string key, string etag)
		{
			Cache.Set($"{key}_{Constants.HttpHeaders.ETag}", Encoding.UTF8.GetBytes(etag));
		}

		public void Save(string key, DateTimeOffset lastModified)
		{
			Cache.Set($"{key}_{Constants.HttpHeaders.LastModified}",
				Encoding.UTF8.GetBytes(lastModified.ToString("d")));
		}
	}
}
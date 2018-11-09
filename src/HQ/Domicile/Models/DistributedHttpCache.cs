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

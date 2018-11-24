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
using Microsoft.Extensions.Caching.Memory;

namespace HQ.Extensions.Caching
{
    public class InMemoryCache : ICache
    {
        private readonly IMemoryCache _cache;

        public InMemoryCache()
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                CompactionPercentage = 0.05,
                ExpirationScanFrequency = TimeSpan.FromMinutes(1.0),
                SizeLimit = null
            });
        }

        public bool Set(string key, object value)
        {
            return Try(() => _cache.Set(key, value, CreateEntry()));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration)
        {
            return Try(() => _cache.Set(key, value, CreateEntry(absoluteExpiration)));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration)
        {
            return Try(() => _cache.Set(key, value, CreateEntry(slidingExpiration: slidingExpiration)));
        }

        public bool Set(string key, object value, ICacheDependency dependency)
        {
            return Try(() => _cache.Set(key, value, CreateEntry(dependency: dependency)));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return Try(() => _cache.Set(key, value, CreateEntry(absoluteExpiration, dependency: dependency)));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return Try(() => _cache.Set(key, value, CreateEntry(slidingExpiration: slidingExpiration, dependency: dependency)));
        }

        public bool Add(string key, object value)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, CreateEntry());
            return true;
        }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, CreateEntry(absoluteExpiration));
            return true;
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, CreateEntry(slidingExpiration: slidingExpiration));
            return true;
        }

        public bool Add(string key, object value, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, CreateEntry(dependency: dependency));
            return true;
        }

        public bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, CreateEntry(absoluteExpiration, dependency: dependency));
            return true;
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, CreateEntry(slidingExpiration: slidingExpiration));
            return true;
        }

        public bool Replace(string key, object value)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, absoluteExpiration)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, slidingExpiration)));
        }

        public bool Replace(string key, object value, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, dependency: dependency)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, absoluteExpiration, dependency)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, slidingExpiration, dependency)));
        }

        public T Get<T>(string key)
        {
            return (T)_cache.Get(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        private static bool Try(Action closure)
        {
            try
            {
                closure.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool RemoveByKeyThen(string key, Func<bool> operation)
        {
            try
            {
                _cache.Remove(key);
                return operation();
            }
            catch
            {
                return false;
            }
        }

        private bool EnsureKeyExistsThen(string key, Func<bool> operation)
        {
            try
            {
                return _cache.Get(key) != null && operation();
            }
            catch
            {
                return false;
            }
        }

        private static MemoryCacheEntryOptions CreateEntry(DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null, long? size = null, ICacheDependency dependency = null)
        {
            var policy = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.Normal,
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = slidingExpiration,
                Size = size
            };

            if (dependency != null)
                policy.AddExpirationToken(dependency.GetChangeToken());

            return policy;
        }
    }
}

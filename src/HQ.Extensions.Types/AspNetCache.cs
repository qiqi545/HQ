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
using System.Collections.Generic;

namespace Depot.AspNet
{
    /// <summary>
    ///     Uses the ASP.NET Cache to cache objects in in-process memory.
    /// </summary>
    public class AspNetCache : ICache
    {
        private readonly Cache _cache = HttpRuntime.Cache;

        public IEnumerable<string> Keys
        {
            get
            {
                var enumerator = _cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Key.ToString();
                }
            }
        }

        public int Count
        {
            get { return _cache.Count; }
        }

        private bool Insert(string key, object value)
        {
            return Try(() => _cache.Insert(key, value));
        }

        private bool Insert(string key, object value, DateTime absoluteExpiration)
        {
            return Try(() => _cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration));
        }

        private bool Insert(string key, object value, TimeSpan slidingExpiration)
        {
            return Try(() => _cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration));
        }

        private bool Insert(string key, object value, ICacheDependency dependency)
        {
            return Try(() => _cache.Insert(key, value, ((AspNetCacheDependency) dependency).Internal));
        }

        private bool Insert(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return Try(() => _cache.Insert(key, value, ((AspNetCacheDependency) dependency).Internal,
                absoluteExpiration, Cache.NoSlidingExpiration));
        }

        private bool Insert(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return Try(() => _cache.Insert(key, value, ((AspNetCacheDependency) dependency).Internal,
                Cache.NoAbsoluteExpiration, slidingExpiration));
        }

        public bool Set(string key, object value)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, absoluteExpiration));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, slidingExpiration));
        }

        public bool Set(string key, object value, ICacheDependency dependency)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, dependency));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, absoluteExpiration, dependency));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return RemoveIfKeyExistsAndThen(key, () => Insert(key, value, slidingExpiration, dependency));
        }

        public bool Add(string key, object value)
        {
            return _cache.Get(key) == null && Insert(key, value);
        }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            return _cache.Get(key) == null && Insert(key, value, absoluteExpiration);
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            return _cache.Get(key) == null && Insert(key, value, slidingExpiration);
        }

        public bool Add(string key, object value, ICacheDependency dependency)
        {
            return _cache.Get(key) == null && Insert(key, value, dependency);
        }

        public bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return _cache.Get(key) == null && Insert(key, value, absoluteExpiration, dependency);
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return _cache.Get(key) == null && Insert(key, value, slidingExpiration, dependency);
        }

        public bool Replace(string key, object value)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveIfKeyExistsAndThen(key, () => Insert(key, value)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration)
        {
            return EnsureKeyExistsAndThen(key,
                () => RemoveIfKeyExistsAndThen(key, () => Insert(key, value, absoluteExpiration)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration)
        {
            return EnsureKeyExistsAndThen(key,
                () => RemoveIfKeyExistsAndThen(key, () => Insert(key, value, slidingExpiration)));
        }

        public bool Replace(string key, object value, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key,
                () => RemoveIfKeyExistsAndThen(key, () => Insert(key, value, dependency)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key,
                () => RemoveIfKeyExistsAndThen(key, () => Insert(key, value, absoluteExpiration, dependency)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsAndThen(key,
                () => RemoveIfKeyExistsAndThen(key, () => Insert(key, value, slidingExpiration, dependency)));
        }

        public T Get<T>(string key)
        {
            return (T) _cache.Get(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            var keys = new List<string>();

            var enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                keys.Add(key);
            }

            foreach (var t in keys)
            {
                _cache.Remove(t);
            }
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

        private bool RemoveIfKeyExistsAndThen(string key, Func<bool> operation)
        {
            try
            {
                if (_cache[key] != null)
                {
                    _cache.Remove(key);
                }

                return operation.Invoke();
            }
            catch
            {
                return false;
            }
        }

        private bool EnsureKeyExistsAndThen(string key, Func<bool> operation)
        {
            try
            {
                return _cache[key] != null && operation.Invoke();
            }
            catch
            {
                return false;
            }
        }
    }
}

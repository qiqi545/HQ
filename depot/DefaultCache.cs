using System;
using Microsoft.Framework.Caching.Memory;

namespace depot
{
    public class DefaultCache : ICache
    {
        private readonly IMemoryCache _cache;

        public DefaultCache()
        {
            var options = new MemoryCacheOptions();
            _cache = new MemoryCache(options);
        }
        
        public bool Set(string key, object value)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy()));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy(absoluteExpiration)));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration)
        {
            return Try(() => _cache.Set(key, value, BuildCachePolicy(slidingExpiration: slidingExpiration)));
        }

        public bool Add(string key, object value)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, BuildCachePolicy());
            return true;
        }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, BuildCachePolicy(absoluteExpiration));
            return true;
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            _cache.Set(key, value, BuildCachePolicy(slidingExpiration: slidingExpiration));
            return true;
        }

        public bool Replace(string key, object value)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value, absoluteExpiration)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration)
        {
            return EnsureKeyExistsAndThen(key, () => RemoveKeyAndThen(key, () => Add(key, value, slidingExpiration)));
        }

        public object Get(string key, Func<object> add, TimeSpan? contentionTimeout = null)
        {
            return this.GetOrAddUntyped(key, add);
        }

        public T Get<T>(string key, Func<T> add, TimeSpan? contentionTimeout = null) where T : class
        {
            
            return this.GetOrAddTyped(key, add);
        }

        public object Get(string key)
        {
            return _cache.Get(key);
        }

        public T Get<T>(string key)
        {
            var item = _cache.Get(key);
            if (item == null)
            {
                return default(T);
            }
            return (T)item;
        }

        public T Remove<T>(string key)
        {
            var removed = _cache.Get<T>(key);
            _cache.Remove(key);
            return removed;
        }

        private MemoryCacheEntryOptions BuildCachePolicy(
            DateTimeOffset? absoluteExpiration = null, 
            TimeSpan? slidingExpiration = null)
        {
            var policy = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.Normal,
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = slidingExpiration,
                PostEvictionCallbacks = { },
                Triggers = { }
            };
            return policy;
        }

        private static bool Try(Action closure)
        {
            try
            {
                closure();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool RemoveKeyAndThen(string key, Func<bool> operation)
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

        private bool EnsureKeyExistsAndThen(string key, Func<bool> operation)
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
    }
}
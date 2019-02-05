using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HQ.Extensions.Caching
{
    internal static class CacheLockScope
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, object>> Locks;
        static CacheLockScope()
        {
            Locks = new ConcurrentDictionary<Type, IDictionary<string, object>>();
        }
        public static object AcquireLock<T>(string key)
        {
            if (!Locks.TryGetValue(typeof(T), out var hash))
            {
                hash = new Dictionary<string, object>();
                Locks.TryAdd(typeof(T), hash);
            }

            if (hash.TryGetValue(key, out var @lock))
                return @lock;

            @lock = new object();
            hash.Add(key, @lock);
            return @lock;
        }
    }
}

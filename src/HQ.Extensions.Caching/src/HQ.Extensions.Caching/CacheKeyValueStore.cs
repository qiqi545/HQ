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

using System.Collections.Generic;

namespace HQ.Extensions.Caching
{
    public class CacheKeyValueStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        private readonly ICache _cache;
        private readonly string _keyGroup;

        public CacheKeyValueStore(ICache cache, string keyGroup = null)
        {
            _cache = cache;
            _keyGroup = keyGroup;
        }

        public TValue this[TKey key] => _cache.Get<TValue>(CacheKey(key));

        public virtual string CacheKey(TKey key)
        {
            return key.ToString();
        }

        public TValue GetOrAdd(TKey key, TValue metric)
        {
            var m = _cache.GetOrAdd(CacheKey(key), metric);
            UpdateKeyGroup(key);
            return m;
        }

        public bool TryGetValue(TKey key, out TValue metric)
        {
            metric = _cache.Get<TValue>(CacheKey(key));
            return metric != null;
        }
        
        public bool Contains(TKey key)
        {
            return TryGetValue(key, out _);
        }

        void IKeyValueStore<TKey, TValue>.AddOrUpdate<T>(TKey key, T value)
        {
            AddOrUpdate(key, value);
        }

        public void AddOrUpdate<T>(TKey key, T metric) where T : TValue
        {
            _cache.Set(CacheKey(key), metric);
            UpdateKeyGroup(key);
        }

        public bool Clear()
        {
            if (!(_cache is IClearable clear))
                return false;
            clear.Clear();
            return true;
        }

        private void UpdateKeyGroup(TKey key)
        {
            if (string.IsNullOrWhiteSpace(_keyGroup))
                return;
            var list = _cache.GetOrAdd(_keyGroup, () => new List<TKey>());
            if (list.Contains(key))
                return;
            list.Add(key);
            _cache.Set(_keyGroup, list);
        }
    }
}

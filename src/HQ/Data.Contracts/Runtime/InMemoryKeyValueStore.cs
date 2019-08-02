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

using System.Collections.Concurrent;

namespace HQ.Data.Contracts.Runtime
{
    public class InMemoryKeyValueStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        protected readonly ConcurrentDictionary<TKey, TValue> Memory;

        public InMemoryKeyValueStore()
        {
            Memory = new ConcurrentDictionary<TKey, TValue>();
        }

        public TValue GetOrAdd(TKey name, TValue metric)
        {
            return Memory.GetOrAdd(name, metric);
        }

        public TValue this[TKey name] => Memory[name];

        public bool TryGetValue(TKey name, out TValue value)
        {
            return Memory.TryGetValue(name, out value);
        }

        public bool Contains(TKey name)
        {
            return Memory.ContainsKey(name);
        }

        public void AddOrUpdate<T>(TKey name, T metric) where T : TValue
        {
            Memory.AddOrUpdate(name, metric, (n, m) => m);
        }

        public bool Clear()
        {
            Memory.Clear();
            return true;
        }
    }
}

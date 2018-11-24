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

namespace HQ.Extensions.Caching
{
    public interface ICache
    {
        /// <summary>
        ///     Adds a new item to the cache if an item with the same key does not exist; if it does exist, it is replaced
        ///     returns <code>true</code> if the item was added successfully and <code>false</code> if was not
        /// </summary>
        bool Set(string key, object value);

        bool Set(string key, object value, DateTime absoluteExpiration);
        bool Set(string key, object value, TimeSpan slidingExpiration);
        bool Set(string key, object value, ICacheDependency dependency);
        bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        ///     Adds a new item to the cache if an item with the same key does not exist;
        ///     returns <code>true</code> if the item was added and <code>false</code> if there was an existing key
        /// </summary>
        bool Add(string key, object value);

        bool Add(string key, object value, DateTime absoluteExpiration);
        bool Add(string key, object value, TimeSpan slidingExpiration);
        bool Add(string key, object value, ICacheDependency dependency);
        bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency);

        /// <summary>
        ///     Replaces an existing item in the cache with a new item if an item with the given key exists;
        ///     returns <code>true</code> if an item was replaced and <code>false</code> if it did not exist
        /// </summary>
        bool Replace(string key, object value);

        bool Replace(string key, object value, DateTime absoluteExpiration);
        bool Replace(string key, object value, TimeSpan slidingExpiration);
        bool Replace(string key, object value, ICacheDependency dependency);
        bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency);
        bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency);

        T Get<T>(string key);
        void Remove(string key);
        void Clear();
    }
}

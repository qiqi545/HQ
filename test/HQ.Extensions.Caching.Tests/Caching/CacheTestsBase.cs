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

using System.Threading;
using HQ.Extensions.Caching.Tests.Extensions;
using Xunit;

namespace HQ.Extensions.Caching.Tests.Caching
{
    public abstract class CacheTestsBase
    {
        protected ICache cache;

        [Fact]
        public virtual void Can_add_and_skip()
        {
            cache.Add("key", "value");
            cache.Add("key", "value2");
            Assert.Equal("value", cache.Get<string>("key")); // Cache did not skip value when adding
        }

        [Fact]
        public virtual void Can_cache_with_absolute_expiry()
        {
            cache.Set("key", "value", 1.Seconds().FromNow());
            Thread.Sleep(1.Seconds());
            Assert.Null(cache.Get<string>("key")); // Cache didn't expire in time
        }

        [Fact]
        public virtual void Can_cache_with_sliding_expiry()
        {
            cache.Set("key", "value", 1.Seconds());
            Thread.Sleep(1.Seconds());
            Assert.Null(cache.Get<string>("key")); // Cache didn't expire in time
        }

        [Fact]
        public virtual void Can_set_and_override()
        {
            cache.Set("key", "value");
            cache.Set("key", "value2");
            Assert.Equal("value2", cache.Get<string>("key")); // Cache did not override value when setting
        }
    }
}

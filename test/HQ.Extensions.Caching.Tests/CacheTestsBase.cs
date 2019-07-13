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
using HQ.Test.Sdk;

namespace HQ.Extensions.Caching.Tests
{
    public abstract class CacheTestsBase : UnitUnderTest
    {
        protected ICache Cache;

        [Test]
        public virtual void Can_add_and_skip()
        {
            Cache.Add("key", "value");
            Cache.Add("key", "value2");
            Assert.Equal("value", Cache.Get<string>("key")); // Cache did not skip value when adding
        }

        [Test]
        public virtual void Can_cache_with_absolute_expiry()
        {
            Cache.Set("key", "value", 1.Seconds().FromNow());
            Thread.Sleep(1.Seconds());
            Assert.Null(Cache.Get<string>("key")); // Cache didn't expire in time
        }

        [Test]
        public virtual void Can_cache_with_sliding_expiry()
        {
            Cache.Set("key", "value", 1.Seconds());
            Thread.Sleep(1.Seconds());
            Assert.Null(Cache.Get<string>("key")); // Cache didn't expire in time
        }

        [Test]
        public virtual void Can_set_and_override()
        {
            Cache.Set("key", "value");
            Cache.Set("key", "value2");
            Assert.Equal("value2", Cache.Get<string>("key")); // Cache did not override value when setting
        }
    }
}

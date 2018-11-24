using Depot.Tests.Extensions;
using Xunit;

namespace Depot.Tests
{
    public abstract class BasicCacheTests
    {
        protected ICache Cache;
        
        [Fact]
        public virtual void Can_cache_with_absolute_expiry()
        {
            Cache.Set("key", "value", 1.Seconds().FromNow());
            System.Threading.Thread.Sleep(1.Seconds());
            Assert.Null(Cache.Get<string>("key"), "Cache didn't expire in time");
        }

        [Fact]
        public virtual void Can_cache_with_sliding_expiry()
        {
            Cache.Set("key", "value", 1.Seconds());
            System.Threading.Thread.Sleep(1.Seconds());
            Assert.Null(Cache.Get<string>("key"), "Cache didn't expire in time");
        }

        [Fact]
        public virtual void Can_set_and_override()
        {
            Cache.Set("key", "value");
            Cache.Set("key", "value2");
            Assert.Equal(Cache.Get<string>("key"), "value2", "Cache did not override value when setting");
        }

        [Fact]
        public virtual void Can_add_and_skip()
        {
            Cache.Add("key", "value");
            Cache.Add("key", "value2");
            Assert.Equal(Cache.Get<string>("key"), "value", "Cache did not skip value when adding");
        }
    }
}
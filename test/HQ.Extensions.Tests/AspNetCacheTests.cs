using Depot.AspNet;

namespace HQ.Extensions.Types
{
    [TestFixture]
    public class AspNetCacheTests : BasicCacheTests
    {
        [SetUp]
        public void SetUp()
        {
            Cache = new AspNetCache();
        }
    }
}

using Depot.AspNet;
using NUnit.Framework;

namespace Depot.Tests
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

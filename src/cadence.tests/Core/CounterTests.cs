using Xunit;

namespace cadence.tests.Core
{
	public class CounterTests : IClassFixture<MetricsFixture>
    {
        [Fact]
        public void Can_count()
        {
            var counter = Metrics.Counter(typeof (CounterTests), "Can_count");
            Assert.NotNull(counter);
            
            counter.Increment(100);
            Assert.Equal(100, counter.Count);
        }
    }
}

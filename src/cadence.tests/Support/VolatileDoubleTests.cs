using System.Diagnostics;
using cadence.Support;
using Xunit;

namespace cadence.tests.Support
{
    public class VolatileDoubleTests
    {
        [Fact]
        public void Can_add_through_wrapper()
        {
            var rate1 = 15.50;
            rate1 += (2 * 10 - rate1);
            Trace.WriteLine(rate1);

            VolatileDouble rate2 = 15.50;
            rate2 += (2*10 - rate2);
            Trace.WriteLine(rate2);

            Assert.Equal(rate1, (double)rate2);
        }
    }
}

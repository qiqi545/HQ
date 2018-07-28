using System.Diagnostics;
using cadence.Support;
using Xunit;

namespace cadence.tests.Support
{
    public class RandomTests
    {
        [Fact]
        public void Can_generate_random_longs()
        {
            for(var i = 0; i < 1000; i++)
            {
                long random = Random.NextLong();
                Trace.WriteLine(random);
            }
        }
    }
}

using System.Diagnostics;
using cadence.Core;
using Xunit;

namespace cadence.tests.Core
{
    public class CLRProfilerTests
    {
        [Fact]
        public void Can_get_heap_usage()
        {
            var heap = CLRProfiler.HeapUsage;
            Assert.NotNull(heap);
            Trace.WriteLine(heap);
        }

        [Fact]
        public void Can_get_uptime()
        {
            var heap = CLRProfiler.Uptime;
            Assert.NotNull(heap);
            Trace.WriteLine(heap);
        }
    }
}

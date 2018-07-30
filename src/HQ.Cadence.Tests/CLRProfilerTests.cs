// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Diagnostics;
using Xunit;

namespace HQ.Cadence.Tests.Core
{
    public class ClrProfilerTests
    {
        [Fact]
        public void Can_get_heap_usage()
        {
            var heap = ClrProfiler.HeapUsage;
            Assert.NotNull(heap);
            Trace.WriteLine(heap);
        }

        [Fact]
        public void Can_get_uptime()
        {
            var heap = ClrProfiler.Uptime;
            Assert.NotNull(heap);
            Trace.WriteLine(heap);
        }
    }
}

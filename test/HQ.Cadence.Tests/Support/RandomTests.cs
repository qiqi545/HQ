// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Diagnostics;
using HQ.Cadence.Internal;
using Xunit;

namespace HQ.Cadence.Tests.Support
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

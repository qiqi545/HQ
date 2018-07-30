// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Xunit;

namespace HQ.Cadence.Tests.Core
{
	public class CounterTests : IClassFixture<MetricsFixture>
    {
	    readonly MetricsFixture _fixture;

	    public CounterTests(MetricsFixture fixture)
	    {
		    _fixture = fixture;
	    }

        [Fact]
        public void Can_count()
        {
            var counter = _fixture.Metrics.Counter(typeof (CounterTests), "Can_count");
            Assert.NotNull(counter);
            
            counter.Increment(100);
            Assert.Equal(100, counter.Count);
        }
    }
}

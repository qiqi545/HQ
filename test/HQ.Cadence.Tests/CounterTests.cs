#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using Xunit;

namespace HQ.Cadence.Tests
{
    public class CounterTests : IClassFixture<MetricsFixture>
    {
        public CounterTests(MetricsFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly MetricsFixture _fixture;

        [Fact]
        public void Can_count()
        {
            var counter = _fixture.Metrics.Counter(typeof(CounterTests), "Can_count");
            Assert.NotNull(counter);

            counter.Increment(100);
            Assert.Equal(100, counter.Count);
        }
    }
}

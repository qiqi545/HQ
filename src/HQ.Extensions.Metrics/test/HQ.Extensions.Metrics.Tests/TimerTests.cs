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

using System.Threading;
using Xunit;

namespace HQ.Extensions.Metrics.Tests
{
    public class TimerTests : IClassFixture<MetricsFixture>
    {
        public TimerTests(MetricsFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly MetricsFixture _fixture;

        [Fact]
        public void Can_time_closure()
        {
            var timer = new TimerMetric(TimeUnit.Seconds, TimeUnit.Seconds);
            var result = timer.Time(() =>
            {
                Thread.Sleep(1);
                return true;
            });
            Assert.True(result.IsStarted);
            Assert.True(result.IsStopped);
            Assert.True(result.StartedAt.HasValue);
            Assert.True(result.StoppedAt.HasValue);
            Assert.True(result);
            Assert.Equal(1, timer.Count);
            Assert.True(timer.Mean > 0);
        }
    }
}

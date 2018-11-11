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

using System.Diagnostics;
using HQ.Cadence.Internal;
using Xunit;

namespace HQ.Cadence.Tests.Support
{
    public class VolatileDoubleTests
    {
        [Fact]
        public void Can_add_through_wrapper()
        {
            var rate1 = 15.50;
            rate1 += 2 * 10 - rate1;
            Trace.WriteLine(rate1);

            VolatileDouble rate2 = 15.50;
            rate2 += 2 * 10 - rate2;
            Trace.WriteLine(rate2);

            Assert.Equal(rate1, (double) rate2);
        }
    }
}

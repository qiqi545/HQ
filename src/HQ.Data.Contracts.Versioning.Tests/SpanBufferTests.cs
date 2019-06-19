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

using System;
using Xunit;

namespace HQ.Data.Contracts.Versioning.Tests
{
    public class SpanBufferTests
    {
        [Fact]
        public void Can_combine_two_spans_in_one_span_buffer()
        {
            var buffer = new SpanBuffer<byte>(2);

            var ao = 0;
            var a = new byte[0].AsSpan();
            a.WriteString(ref ao, "A");

            var bo = 0;
            var b = new byte[0].AsSpan();
            b.WriteString(ref bo, "B");

            buffer.Add(a);
            buffer.Add(b);

            var length = 1 + sizeof(int) + 1 + /* A */
                         1 + sizeof(int) + 1; /* B */

            Assert.Equal(length, buffer.Length);
        }

        [Fact]
        public void Empty_is_empty()
        {
            var buffer = new SpanBuffer<byte>(2);

            var a = new byte[0].AsSpan();
            var b = new byte[0].AsSpan();

            buffer.Add(a);
            buffer.Add(b);

            Assert.True(buffer.IsEmpty);
        }
    }
}

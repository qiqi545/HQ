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
using System.Text;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Fixtures;

namespace HQ.Data.Streaming.Tests
{
    public class ParsingTests : UnitUnderTest
    {
        [Test]
        public void Can_parse_line_values()
        {
            var values = 0;
            var encoding = Encoding.UTF8;
            using (var fixture = new FlatFileFixture(100000, encoding, ","))
            {
                var sw = Stopwatch.StartNew();
                unsafe
                {
                    LineReader.ReadLines(fixture.FileStream, encoding, ",", (n, i, start, length, e, m) =>
                    {
                        values++;
                        e.TryParse(start, length, out bool _);
                    });
                }

                Trace.WriteLine($"{values} cells took {sw.Elapsed} to parse.");
            }
        }
    }
}

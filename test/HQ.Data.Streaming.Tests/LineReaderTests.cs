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
using System.Diagnostics;
using System.Text;
using HQ.Touchstone;
using HQ.Touchstone.Fixtures;
using HQ.Touchstone.Xunit;

namespace HQ.Data.Streaming.Tests
{
    public class LineReaderTests : UnitUnderTest
    {
        [Test]
        public void Can_read_lines()
        {
            using (var fixture = new FlatFileFixture(10000, Encoding.UTF8))
            {
                var lines = 0UL;
                var sw = Stopwatch.StartNew();
                LineReader.ReadLines(fixture.FileStream, Encoding.UTF8, (lineNumber, line, metrics) =>
                {
                    Assert.Single(line.Split(Environment.NewLine));
                    lines = lineNumber;
                });
                Trace.WriteLine($"{lines} lines took {sw.Elapsed} to read.");
            }
        }

        [Test]
        public void Can_count_lines()
        {
            using (var fixture = new FlatFileFixture(10000, Encoding.UTF8))
            {
                var sw = Stopwatch.StartNew();
                var lines = LineReader.CountLines(fixture.FileStream, Encoding.UTF8);
                Trace.WriteLine($"{lines} lines took {sw.Elapsed} to read.");
            }
        }
    }
}

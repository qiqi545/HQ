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
using HQ.Data.Streaming.Fields;
using HQ.Data.Streaming.Internal;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Fixtures;
using HQ.Test.Sdk.Xunit;

namespace HQ.Data.Streaming.Tests
{
    public class LineReaderTests : UnitUnderTest
    {
        [Test]
        public void Can_read_string_lines()
        {
            using (var fixture = new FlatFileFixture(10000, Encoding.UTF8))
            {
                var lines = 0L;
                var sw = Stopwatch.StartNew();
                LineReader.ReadLines(fixture.FileStream, Encoding.UTF8, (lineNumber, line, metrics) =>
                {
                    Assert.NotNull(line);
                    lines = lineNumber;
                });
                Trace.WriteLine($"{lines} lines took {sw.Elapsed} to read.");
            }
        }

        [Test]
        public void Can_read_constructor_lines()
        {
            using (var fixture = new FlatFileFixture(10000, Encoding.UTF8))
            {
                var lines = 0UL;
                var sw = Stopwatch.StartNew();
                foreach(var ctor in LineReader.ReadLines(fixture.FileStream, Encoding.UTF8, "|"))
                {
                    var row = new DummyData(ctor, Encoding.UTF8, Encoding.UTF8.GetSeparatorBuffer("|"));
                    Assert.NotNull(row.someField.Value);
                    lines++;
                }
                Trace.WriteLine($"{lines} lines took {sw.Elapsed} to read.");
            }
        }

        public ref struct DummyData
        {
            public StringField someField;
            public StringField extraFields;

            public DummyData(LineConstructor constructor, Encoding encoding,  byte[] separator)
            {
                someField = default;
                extraFields = default;

                SetFromLineConstructor(constructor, encoding, separator);
            }

            private unsafe void SetFromLineConstructor(LineConstructor constructor, Encoding encoding, byte[] separator)
            {
                fixed (byte* from = constructor.start)
                {
                    var start = from;
                    var length = constructor.length;
                    var column = 0;
                    while (true)
                    {
                        var line = new ReadOnlySpan<byte>(start, length);
                        var next = line.IndexOf(separator);
                        if (next == -1)
                        {
                            if (line.IndexOf(Constants.CarriageReturn) > -1)
                                someField = new StringField(start, length - 2, encoding);
                            else if (line.IndexOf(Constants.LineFeed) > -1)
                                someField = new StringField(start, length - 1, encoding);
                            else
                                someField = new StringField(start, length, encoding);
                            break;
                        }

                        var consumed = next + separator.Length;
                        length -= next + separator.Length;

                        switch (column)
                        {
                            case 0:
                                someField = new StringField(start, next, encoding);
                                break;
                            default:
                                extraFields = extraFields.Initialized
                                    ? extraFields.AddLength(next)
                                    : new StringField(start, next, encoding);
                                break;
                        }

                        start += consumed;
                        column++;
                    }
                }
            }
        }

        [Test]
        public void Can_count_lines()
        {
            var expected = 10000L;
            using (var fixture = new FlatFileFixture((int)expected, Encoding.UTF8))
            {
                var sw = Stopwatch.StartNew();
                var lines = LineReader.CountLines(fixture.FileStream, Encoding.UTF8);
                Assert.Equal(expected, lines);
                Trace.WriteLine($"{lines} lines took {sw.Elapsed} to read.");
            }
        }

        [Test]
        public void Can_count_lines_ranged()
        {
            var expected = 10000L;
            using (var fixture = new FlatFileFixture((int)expected, Encoding.UTF8))
            {
                var sw = Stopwatch.StartNew();
                var lines = LineReader.CountLines(fixture.FileStream, Encoding.UTF8);
                Assert.Equal(expected, lines);
                Trace.WriteLine($"{lines} lines took {sw.Elapsed} to read.");
            }
        }
    }
}

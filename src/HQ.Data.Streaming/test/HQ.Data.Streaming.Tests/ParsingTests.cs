using System.Diagnostics;
using System.Text;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Fixtures;
using HQ.Test.Sdk.Xunit;

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

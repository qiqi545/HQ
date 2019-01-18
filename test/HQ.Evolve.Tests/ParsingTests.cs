using System.Diagnostics;
using System.Text;
using HQ.Touchstone;
using HQ.Touchstone.Fixtures;
using HQ.Touchstone.Xunit;

namespace HQ.Evolve.Tests
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

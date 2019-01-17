using System.Diagnostics;
using System.Text;
using HQ.Touchstone;
using HQ.Touchstone.Fixtures;
using HQ.Touchstone.Xunit;

namespace HQ.Evolve.Tests
{
    public class LineValuesReaderTests : UnitUnderTest
    {
        [Test]
        public void Can_read_line_values()
        {
            var values = 0;
            using (var fixture = new FlatFileFixture(100000, Encoding.UTF8, ","))
            {
                var sw = Stopwatch.StartNew();
                LineReader.ReadLines(fixture.FileStream, Encoding.UTF8, ",", (i, v, e) =>
                {
                    values++;
                });
                Trace.WriteLine($"{values} cells took {sw.Elapsed} to read.");
            }
        }
    }
}

using System.Collections.Generic;
using Xunit;

namespace HQ.Common.Tests
{
    public class SelfEnumerationTests
    {
        [Fact]
        public void Can_enumerate()
        {
            var expected = new List<string> {"A", "B", "C"};
            var actual = new List<string>();

            var enumerable = expected.SelfEnumerate();
            foreach (var value in enumerable)
                actual.Add(value);

            actual.Clear();
            foreach (var value in enumerable)
                actual.Add(value);

            Assert.Equal(expected, actual);
            Assert.Equal(actual, enumerable.AsList);
        }
    }
}

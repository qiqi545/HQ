using System.Collections.Generic;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Common.Tests
{
    public class SelfEnumerationTests : UnitUnderTest
    {
        [Test]
        public void Can_enumerate()
        {
            var expected = new List<string> {"A", "B", "C"};
            var actual = new List<string>();

            var enumerable = expected.Enumerate();
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

using HQ.Common.Numerics;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Common.Tests.Numerics
{
    public class FixedNumberTests : UnitUnderTest
    {
        [Test]
        public void BasicTests_Splits()
        {
            var v = new FixedNumber(12312345678, 3);
            Assert.Equal(12312345, v.WholePart);
            Assert.Equal(678, v.Fraction);
            Assert.Equal(11, v.Precision);
            Assert.Equal(3, v.Scale);
        }

        [Test]
        public void BasicTests_Operators()
        {
            var v = new FixedNumber(123123, 3);
            Assert.Equal(123, v.WholePart);
            Assert.Equal(123, v.Fraction);

            v += 100;
            Assert.Equal(223, v.WholePart);
            Assert.Equal(123, v.Fraction);

            v -= 100;
            Assert.Equal(123, v.WholePart);
            Assert.Equal(123, v.Fraction);
        }

        [Test]
        public void BasicTests_ToString()
        {
            var v = new FixedNumber(123123, 3);
            var value = v.ToString();
            Assert.Equal("123.123", value);
        }

        [DataDrivenTest]
        [InlineData("123.123", 123, 123)]
        [InlineData("123.123000", 123, 123)]
        [InlineData("-123.123", -123, -123)]
        public void BasicTests_Parsing(string s, int wholePart, int fraction)
        {
            Assert.True(FixedNumber.TryParse(s, out var v));
            Assert.Equal(wholePart, v.WholePart);
            Assert.Equal(fraction, v.Fraction);
        }

        [Test]
        public void BasicTests_Conversion()
        {
            FixedNumber @float = 123.123f;
            Assert.Equal(123, @float.WholePart);
            Assert.Equal(123, @float.Fraction);
            Assert.Equal(6, @float.Precision);
            Assert.Equal(3, @float.Scale);

            FixedNumber @double = 123.123d;
            Assert.Equal(123, @double.WholePart);
            Assert.Equal(123, @double.Fraction);
            Assert.Equal(6, @double.Precision);
            Assert.Equal(3, @double.Scale);

            FixedNumber @decimal = 123.123m;
            Assert.Equal(123, @decimal.WholePart);
            Assert.Equal(123, @decimal.Fraction);
            Assert.Equal(6, @decimal.Precision);
            Assert.Equal(3, @decimal.Scale);
        }
    }
}

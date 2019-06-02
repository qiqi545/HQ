using HQ.Extensions.CodeGeneration.Scripting;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Xunit;

namespace HQ.Extensions.CodeGeneration.Tests.Scripting
{
    public class ComputedStringTests : UnitUnderTest
    {
        [Test]
        public void Can_compute_string_from_self_referenced_properties()
        {
            var instance = new Foo();
            instance.Bar = "Fizz";
            instance.Baz = "Buzz";

            var actual = ComputedString.Compute(instance, "{{Bar}}{{Baz}}");
            Assert.Equal("FizzBuzz", actual);
        }

        public class Foo
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}

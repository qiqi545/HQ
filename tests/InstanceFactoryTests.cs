using System;
using Xunit;

namespace hq.compiler.tests
{
    public class InstanceFactoryTests
    {
        [Fact]
        public void Can_replace_activator()
        {
            var foo1 = InstanceFactory.Instance.CreateInstance<Foo>();
            var foo2 = Activator.CreateInstance<Foo>();
            Assert.NotNull(foo1);
            Assert.NotNull(foo2);
        }

        public class Foo { }
    }
}

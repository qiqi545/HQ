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

using Xunit;

namespace HQ.Extensions.DependencyInjection.Tests
{
    public class ResolveTests : IClassFixture<DependencyContainerFixture>
    {
        public ResolveTests(DependencyContainerFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DependencyContainerFixture _fixture;

        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }

        public class Bar
        {
            public Bar(IFoo baz)
            {
                Baz = baz;
            }

            public IFoo Baz { get; set; }
        }

        [Fact]
        public void Can_resolve_arbitrary_types()
        {
            _fixture.C.Register<IFoo>(() => new Foo(), Lifetime.Permanent);

            var first = _fixture.C.Resolve<Bar>();
            var second = _fixture.C.Resolve<Bar>();

            Assert.NotSame(first, second);
            Assert.Same(first.Baz, second.Baz);
        }

        [Fact]
        public void Can_resolve_instance_twice_with_same_reference()
        {
            var instance = new Foo();
            _fixture.C.Register<IFoo>(instance);

            var first = _fixture.C.Resolve<IFoo>();
            var second = _fixture.C.Resolve<IFoo>();

            Assert.Same(first, second);
        }

        [Fact]
        public void Can_resolve_singleton_twice_with_same_reference()
        {
            _fixture.C.Register<IFoo>(() => new Foo(), Lifetime.Permanent);

            var first = _fixture.C.Resolve<IFoo>();
            var second = _fixture.C.Resolve<IFoo>();

            Assert.Same(first, second);
        }

        [Fact]
        public void Can_resolve_transient_twice_with_different_references()
        {
            _fixture.C.Register<IFoo>(() => new Foo());

            var first = _fixture.C.Resolve<IFoo>();
            var second = _fixture.C.Resolve<IFoo>();

            Assert.NotSame(first, second);
        }
    }
}

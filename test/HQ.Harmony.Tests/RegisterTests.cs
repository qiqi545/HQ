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

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HQ.Harmony.Tests
{
    public class RegisterTests : IClassFixture<HarmonyContainerFixture>
    {
        public RegisterTests(HarmonyContainerFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly HarmonyContainerFixture _fixture;

        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }

        public class OtherFoo : IFoo
        {
        }

        [Fact]
        public void Can_register_twice_and_get_back_a_collection()
        {
            _fixture.C.Register<IFoo>(() => new Foo(), Lifetime.Permanent);
            _fixture.C.Register<IFoo>(() => new OtherFoo(), Lifetime.Permanent);

            // strong-typed
            var strong = _fixture.C.ResolveAll<IFoo>();
            Assert.NotNull(strong);
            Assert.Equal(2, strong.Count());

            // weak-typed
            var weak = _fixture.C.ResolveAll(typeof(IFoo)).Cast<IFoo>();
            Assert.NotNull(weak);
            Assert.Equal(2, weak.Count());

            // implied
            var implied = _fixture.C.Resolve<IEnumerable<IFoo>>();
            Assert.NotNull(implied);
            Assert.Equal(2, implied.Count());
        }
    }
}

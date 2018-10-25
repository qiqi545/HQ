// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

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
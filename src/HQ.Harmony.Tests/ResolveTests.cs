﻿// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Xunit;

namespace HQ.Harmony.Tests
{
	public class ResolveTests : IClassFixture<HarmonyContainerFixture>
	{
		public ResolveTests(HarmonyContainerFixture fixture)
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
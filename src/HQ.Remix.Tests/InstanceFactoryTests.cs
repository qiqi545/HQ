// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using Xunit;

namespace HQ.Remix.Tests
{
	public class InstanceFactoryTests
	{
		public class Foo { }

		[Fact]
		public void Can_replace_activator()
		{
			var foo1 = InstanceFactory.Instance.CreateInstance<Foo>();
			var foo2 = Activator.CreateInstance<Foo>();
			Assert.NotNull(foo1);
			Assert.NotNull(foo2);
		}

		public class Class { }

		[Fact]
		public void Can_use_expression_factory()
		{
			var ctor = typeof(Class).GetConstructor(Type.EmptyTypes);
			var activator = InstanceFactory.CompiledExpressionFactory.Build(ctor);
			for (var i = 0; i < 100000; i++)
			{
				activator();
			}
		}

		[Fact]
		public void Can_use_dynamic_factory()
		{
			var ctor = typeof(Class).GetConstructor(Type.EmptyTypes);
			var activator = InstanceFactory.DynamicMethodFactory.Build(typeof(Class), ctor);
			for (var i = 0; i < 100000; i++)
			{
				activator();
			}
		}
	}
}
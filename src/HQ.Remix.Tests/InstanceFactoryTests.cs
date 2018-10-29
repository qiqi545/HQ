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
	}
}
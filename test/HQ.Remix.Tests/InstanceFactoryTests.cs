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

using System;
using Xunit;

namespace HQ.Remix.Tests
{
	public class InstanceFactoryTests
	{
		public class Foo
		{
		}

		public class Class
		{
		}

		[Fact]
		public void Can_replace_activator()
		{
			var foo1 = InstanceFactory.Default.CreateInstance<Foo>();
			var foo2 = Activator.CreateInstance<Foo>();
			Assert.NotNull(foo1);
			Assert.NotNull(foo2);
		}

		[Fact]
		public void Can_use_dynamic_factory()
		{
			var ctor = typeof(Class).GetConstructor(Type.EmptyTypes);
			var activator = InstanceFactory.DynamicMethodFactory.Build(typeof(Class), ctor);
			for (var i = 0; i < 100000; i++) activator();
		}

		[Fact]
		public void Can_use_expression_factory()
		{
			var ctor = typeof(Class).GetConstructor(Type.EmptyTypes);
			var activator = InstanceFactory.CompiledExpressionFactory.Build(ctor);
			for (var i = 0; i < 100000; i++) activator();
		}
	}
}
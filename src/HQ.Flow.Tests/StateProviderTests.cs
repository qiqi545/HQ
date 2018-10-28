// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Flow.Tests.Fixtures;
using HQ.Flow.Tests.States;
using Xunit;

namespace HQ.Flow.Tests
{
	public class StateProviderTests
	{
		[Fact]
		public void Calling_setup_after_clear_does_not_throw()
		{
			using (new StateProviderFixture())
			{
				StateProvider.Setup(typeof(NoStates));
				StateProvider.Clear();
				StateProvider.Setup(typeof(NoStates));
			}
		}

		[Fact]
		public void Calling_setup_once_does_not_throw()
		{
			using (new StateProviderFixture())
			{
				StateProvider.Setup(typeof(NoStates));
			}
		}

		[Fact]
		public void Calling_setup_twice_throws()
		{
			using (new StateProviderFixture())
			{
				Assert.Throws<AlreadyInitializedException>(() =>
				{
					StateProvider.Setup(typeof(NoStates));
					StateProvider.Setup(typeof(NoStates));
				});
			}
		}

		[Fact]
		public void Clear_is_idempotent()
		{
			using (new StateProviderFixture())
			{
				StateProvider.Clear();
				StateProvider.Clear();
				StateProvider.Clear();
			}
		}

		[Fact]
		public void Duplicate_state_method_throws()
		{
			using (new StateProviderFixture())
			{
				Assert.Throws<DuplicateStateMethodException>(() => { StateProvider.Setup<DuplicateStateMethods>(); });
			}
		}

		[Fact]
		public void Missing_state_method_throws()
		{
			using (new StateProviderFixture())
			{
				Assert.Throws<UnusedStateMethodsException>(() =>
				{
					StateProvider.Setup<MissingStateForStateMethod>();
				});
			}
		}
	}
}
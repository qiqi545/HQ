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

using HQ.Extensions.Messaging.Tests.Fixtures;
using HQ.Extensions.Messaging.Tests.States;
using Xunit;

namespace HQ.Extensions.Messaging.Tests
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

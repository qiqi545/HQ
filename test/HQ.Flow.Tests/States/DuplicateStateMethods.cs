// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Flow.Tests.States
{
    public class DuplicateStateMethods : StateMachine<object>
    {
        #region StateA

        public class StateA : State
        {
        }

        private void StateA_BeginState(object stateData, State previousState)
        {
        }

        private void State_StateA_BeginState(object stateData, State previousState)
        {
        }

        #endregion
    }
}

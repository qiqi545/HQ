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

namespace HQ.Flow.Tests.States
{
    public class ThreeValidStaticStates : StateMachine<ThreeValidStatesData>
    {
        #region StateA

        public class StateA : State, INamedState
        {
            public string Name => "StateA";

            private void State_StateA_BeginState(ThreeValidStatesData stateData, State previousState)
            {
                stateData.BeginStateA = true;
            }

            private void State_StateA_EndState(ThreeValidStatesData stateData, State nextState)
            {
                stateData.EndStateA = true;
            }

            private void State_StateA_Update(ThreeValidStatesData stateData)
            {
                stateData.TicksA++;
            }
        }

        #endregion

        #region StateB

        public class StateB : State
        {
            private void StateB_BeginState(ThreeValidStatesData stateData, State previousState)
            {
                stateData.BeginStateB = true;
            }

            private void StateB_EndState(ThreeValidStatesData stateData, State nextState)
            {
                stateData.EndStateB = true;
            }

            private void State_StateB_Update(ThreeValidStatesData stateData)
            {
                stateData.TicksB++;
            }
        }

        #endregion

        #region StateC

        public class StateC : State, INamedState
        {
            public string Name => "StateC";

            private bool State_StateC_BeforeBeginState(ThreeValidStatesData stateData, State nextState)
            {
                return stateData.AllowBeginStateC;
            }

            private void BeginState(ThreeValidStatesData stateData, State previousState)
            {
            }

            private bool State_StateC_BeforeEndState(ThreeValidStatesData stateData, State previousState)
            {
                return stateData.AllowEndStateC;
            }

            private void EndState(ThreeValidStatesData stateData, State nextState)
            {
            }
        }

        #endregion
    }
}

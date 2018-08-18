namespace HQ.Flow.Tests.States
{
	public class ThreeValidStates : StateMachine<ThreeValidStatesData>
	{
		#region StateA

		public class StateA : State { }

		private void StateA_BeginState(ThreeValidStatesData stateData, State previousState)
		{
			stateData.BeginStateA = true;
		}

		private void State_StateA_EndState(ThreeValidStatesData stateData, State nextState)
		{
			stateData.EndStateA = true;
		}

		#endregion

		#region StateB

		public class StateB : State { }

		private void State_StateB_BeginState(ThreeValidStatesData threeValidStatesData, State previousState)
		{
			threeValidStatesData.BeginStateB = true;
		}

		private void State_StateB_EndState(ThreeValidStatesData threeValidStatesData, State nextState)
		{
			threeValidStatesData.EndStateB = true;
		}

		#endregion

		#region StateC

		public class StateC : State { }

		private void State_StateC_BeginState(ThreeValidStatesData threeValidStatesData, State previousState)
		{

		}

		private void State_StateC_EndState(ThreeValidStatesData threeValidStatesData, State nextState)
		{

		}

		#endregion
	}
}
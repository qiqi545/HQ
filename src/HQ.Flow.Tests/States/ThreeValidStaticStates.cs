namespace HQ.Flow.Tests.States
{
	public class ThreeValidStaticStates : StateMachine<ThreeValidStatesData>
	{
		#region StateA

		public class StateA : State
		{
			private void State_StateA_BeginState(ThreeValidStatesData stateData, State previousState)
			{
				stateData.BeginStateA = true;
			}
			
			private void State_StateA_EndState(ThreeValidStatesData stateData, State nextState)
			{
				stateData.EndStateA = true;
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
		}
		
		#endregion

		#region StateC

		public class StateC : State
		{
			private void BeginState(ThreeValidStatesData stateData, State previousState)
			{
				
			}

			private void EndState(ThreeValidStatesData stateData, State nextState)
			{ 
				
			}
		}
		
		#endregion
	}
}
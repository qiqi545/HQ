namespace HQ.Flow.Tests
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
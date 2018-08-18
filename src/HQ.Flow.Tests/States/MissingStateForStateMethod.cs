namespace HQ.Flow.Tests
{
	public class MissingStateForStateMethod : StateMachine<object>
	{
		private void StateA_BeginState(object userData, State previousState) { }
	}
}
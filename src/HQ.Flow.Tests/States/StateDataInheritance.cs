namespace HQ.Flow.Tests.States
{
	public class StateDataInheritance : StateMachine<BaseStateData>
	{
		public class StateA : State { }

		private void State_StateA_BeginState(DerivedStateData stateData, State previousState)
		{
			stateData.A = false;
			stateData.B = true;
		}
	}

	public class BaseStateData
	{
		public bool A { get; set; }
	}

	public class DerivedStateData : BaseStateData
	{
		public bool B { get; set; }
	}
}

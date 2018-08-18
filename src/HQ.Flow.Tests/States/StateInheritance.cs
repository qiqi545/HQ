using System;

namespace HQ.Flow.Tests.States
{
	public class StateInheritance : StateMachine<object>
	{
		public class StateA : DerivedState
		{
			public override int Sprockets => 5;
			public override int Widgets => 10;
		}

		private void State_StateA_BeginState(object stateData, State previousState)
		{
			var current = (DerivedState)CurrentState;
			if (current.Widgets != 10 || current.Sprockets != 5)
				throw new Exception("widgets had unexpected value");
		}
	}
}
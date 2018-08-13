using System;

namespace HQ.Flow
{
	public class StateMachine : StateProvider
	{
		public StateMachine()
		{
			CurrentState = GetState<State>();
		}

		public new class MethodTable : StateProvider.MethodTable
		{
			[AlwaysNullChecked]
			public Action<StateMachine, UserData, State> BeginState;

			[AlwaysNullChecked]
			public Action<StateMachine, UserData, State> EndState;
		}

		public MethodTable StateMethods => (MethodTable)CurrentState.methodTable;
		public State CurrentState { get; private set; }

		public override string ToString()
		{
			return $"{GetType().Name} ({(CurrentState != null ? CurrentState.GetType().Name : "(null)")})";
		}

		public void SetState<TState>(UserData stateData, bool allowStateRestart = false) where TState : State, new()
		{
			DirectlySetState(GetState<TState>(), stateData, allowStateRestart);
		}

		private void DirectlySetState(State nextState, UserData stateData, bool allowStateRestart)
		{
			if (!allowStateRestart && ReferenceEquals(CurrentState, nextState))
				return;

			StateMethods.EndState?.Invoke(this, stateData, nextState);
			var previousState = CurrentState;

			CurrentState = nextState;
			StateMethods.BeginState?.Invoke(this, stateData, previousState);
		}
	}
}

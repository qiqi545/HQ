using System;
using System.Diagnostics;

namespace HQ.Flow
{
	[DebuggerDisplay("{" + nameof(DisplayName) + "}")]
	public class StateMachine<TStateData> : StateProvider where TStateData : class
	{
		public string DisplayName => $"{GetType().Name} ({(CurrentState != null ? CurrentState.GetType().Name : "(null)")})";

		public StateMachine()
		{
			CurrentState = GetState<State>();
		}

		public new class MethodTable : StateProvider.MethodTable
		{
			[AlwaysNullChecked]
			public Func<StateMachine<TStateData>, TStateData, State, bool> PreCondition;

			[AlwaysNullChecked]
			public Action<StateMachine<TStateData>, TStateData, State> BeginState;

			[AlwaysNullChecked]
			public Action<StateMachine<TStateData>, TStateData> Update;

			[AlwaysNullChecked]
			public Action<StateMachine<TStateData>, TStateData, State> EndState;
		}

		public MethodTable StateMethods => (MethodTable)CurrentState.methodTable;
		public State CurrentState { get; private set; }

		public void SetState<TState>(TStateData stateData = null, bool allowStateRestart = false) where TState : State, new()
		{
			DirectlySetState(GetState<TState>(), stateData, allowStateRestart);
		}

		[IgnoreStateMethod]
		public virtual void Update(TStateData stateData)
		{
			StateMethods.Update?.Invoke(this, stateData);
		}

		private void DirectlySetState(State nextState, TStateData stateData, bool allowStateRestart)
		{
			if (!allowStateRestart && ReferenceEquals(CurrentState, nextState))
				return;

			if (nextState?.methodTable is MethodTable methodTable)
			{
				var precondition = methodTable.PreCondition?.Invoke(this, stateData, nextState);
				if (precondition.HasValue && !precondition.Value)
					return;
			}
			
			StateMethods.EndState?.Invoke(this, stateData, nextState);
			var previousState = CurrentState;

			CurrentState = nextState;
			StateMethods.BeginState?.Invoke(this, stateData, previousState);
		}
	}
}

using System;

namespace HQ.Flow
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	internal class DefaultUpdateAttribute : Attribute { }

	public class StateMachine<TStateData> : StateProvider where TStateData : class
	{
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

		public override string ToString()
		{
			return $"{GetType().Name} ({(CurrentState != null ? CurrentState.GetType().Name : "(null)")})";
		}

		public void SetState<TState>(TStateData stateData = null, bool allowStateRestart = false) where TState : State, new()
		{
			DirectlySetState(GetState<TState>(), stateData, allowStateRestart);
		}

		[DefaultUpdate]
		public virtual void Update(TStateData stateData)
		{
			StateMethods?.Update(this, stateData);
		}

		private void DirectlySetState(State nextState, TStateData stateData, bool allowStateRestart)
		{
			if (!allowStateRestart && ReferenceEquals(CurrentState, nextState))
				return;

			var precondition = StateMethods.PreCondition?.Invoke(this, stateData, nextState);
			if (precondition.HasValue && !precondition.Value)
				return;

			StateMethods.EndState?.Invoke(this, stateData, nextState);
			var previousState = CurrentState;

			CurrentState = nextState;
			StateMethods.BeginState?.Invoke(this, stateData, previousState);
		}
	}
}

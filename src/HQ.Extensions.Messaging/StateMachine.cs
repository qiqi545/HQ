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

using System;
using System.Diagnostics;

namespace HQ.Extensions.Messaging
{
    [DebuggerDisplay("{" + nameof(DisplayName) + "}")]
    public class StateMachine<TStateData> : StateProvider where TStateData : class
    {
        public StateMachine()
        {
            CurrentState = GetState<State>();
        }

        public string DisplayName =>
            $"{GetType().Name} ({(CurrentState != null ? CurrentState.GetType().Name : "(null)")})";

        public MethodTable StateMethods => (MethodTable) CurrentState.methodTable;
        public State CurrentState { get; private set; }

        public void SetState<TState>(TStateData stateData = null, bool allowStateRestart = false)
            where TState : State, new()
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

            {
                if (CurrentState?.methodTable is MethodTable methodTable)
                {
                    var beforeEnd = methodTable.BeforeEndState?.Invoke(this, stateData, CurrentState);
                    if (beforeEnd.HasValue && !beforeEnd.Value)
                        return;
                }
            }

            {
                if (nextState?.methodTable is MethodTable methodTable)
                {
                    var beforeBegin = methodTable.BeforeBeginState?.Invoke(this, stateData, nextState);
                    if (beforeBegin.HasValue && !beforeBegin.Value)
                        return;
                }
            }

            StateMethods.EndState?.Invoke(this, stateData, nextState);
            var previousState = CurrentState;

            CurrentState = nextState;
            StateMethods.BeginState?.Invoke(this, stateData, previousState);
        }

        public new class MethodTable : StateProvider.MethodTable
        {
            [AlwaysNullChecked] public Func<StateMachine<TStateData>, TStateData, State, bool> BeforeBeginState;

            [AlwaysNullChecked] public Func<StateMachine<TStateData>, TStateData, State, bool> BeforeEndState;

            [AlwaysNullChecked] public Action<StateMachine<TStateData>, TStateData, State> BeginState;

            [AlwaysNullChecked] public Action<StateMachine<TStateData>, TStateData, State> EndState;

            [AlwaysNullChecked] public Action<StateMachine<TStateData>, TStateData> Update;
        }
    }
}

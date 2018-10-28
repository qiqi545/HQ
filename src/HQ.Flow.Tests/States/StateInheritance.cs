// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Tests.States
{
	public class StateInheritance : StateMachine<object>
	{
		private void State_StateA_BeginState(object stateData, State previousState)
		{
			var current = (DerivedState) CurrentState;
			if (current.Widgets != 10 || current.Sprockets != 5)
				throw new Exception("widgets had unexpected value");
		}

		public class StateA : DerivedState
		{
			public override int Sprockets => 5;
			public override int Widgets => 10;
		}
	}
}
// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Flow.Tests.States
{
	public class DerivedState : StateProvider.State
	{
		public virtual int Sprockets => 10;
		public virtual int Widgets => 5;
	}
}
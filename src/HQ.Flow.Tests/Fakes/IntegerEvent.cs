// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Tests.Fakes
{
	public class IntegerEvent : IEquatable<IntegerEvent>
	{
		public IntegerEvent()
		{
		}

		public IntegerEvent(int value)
		{
			Value = value;
		}

		public int Value { get; set; }

		public bool Equals(IntegerEvent other)
		{
			if (other == null) return false;
			return other.Value.Equals(Value);
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
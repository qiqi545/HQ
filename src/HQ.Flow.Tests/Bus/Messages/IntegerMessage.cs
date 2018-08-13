// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Tests.Messages
{
	public class IntegerMessage : IEquatable<IntegerMessage>
	{
		public IntegerMessage()
		{
		}

		public IntegerMessage(int value)
		{
			Value = value;
		}

		public int Value { get; set; }

		public bool Equals(IntegerMessage other)
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
using System;

namespace HQ.Flow.Tests.Messages
{
	public class IntegerMessage : IEquatable<IntegerMessage>
	{
		public int Value { get; set; }

		public IntegerMessage() { }

		public IntegerMessage(int value)
		{
			Value = value;
		}

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
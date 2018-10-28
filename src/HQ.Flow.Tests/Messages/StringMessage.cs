using System;

namespace HQ.Flow.Tests.Messages
{
	public class StringMessage : IEquatable<StringMessage>
	{
		public string Text { get; set; }

		public StringMessage() { }

		public StringMessage(string text)
		{
			Text = text;
		}

		public bool Equals(StringMessage other)
		{
			if (other == null) return false;
			return Text != null && other.Text.Equals(Text);
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
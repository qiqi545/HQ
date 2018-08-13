// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Tests.Messages
{
	public class StringMessage : IEquatable<StringMessage>
	{
		public StringMessage()
		{
		}

		public StringMessage(string text)
		{
			Text = text;
		}

		public string Text { get; set; }

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
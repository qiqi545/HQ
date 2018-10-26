// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow.Tests.Fakes
{
	public class StringEvent : IEquatable<StringEvent>
	{
		public StringEvent()
		{
		}

		public StringEvent(string text)
		{
			Text = text;
		}

		public string Text { get; set; }

		public bool Equals(StringEvent other)
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
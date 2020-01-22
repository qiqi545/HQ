using System;
using System.Collections.Generic;
using System.Text;

namespace HQ.Data.Streaming
{
	public unsafe delegate void NewLine(long lineNumber, bool partial, byte* start, int length, Encoding encoding);

	public unsafe delegate void NewValue(long lineNumber, int index, byte* start, int length, Encoding encoding);

	public delegate void NewLineAsString(long lineNumber, string value);

	public delegate void NewValueAsSpan(long lineNumber, int index, ReadOnlySpan<byte> value, Encoding encoding);
}

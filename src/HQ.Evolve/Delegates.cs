using System;
using System.Text;

namespace HQ.Evolve
{
    public unsafe delegate void NewLine(ulong lineNumber, byte* start, int length, Encoding encoding);

    public delegate void NewLineAsString(ulong lineNumber, string value);

    public delegate void NewValue(int index, ReadOnlySpan<byte> value, Encoding encoding);

    public delegate void NewValueAsString(int index, string value);
}

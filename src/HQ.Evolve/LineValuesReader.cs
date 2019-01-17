using System;
using System.Text;
using HQ.Evolve.Internal;

namespace HQ.Evolve
{
    public static class LineValuesReader
    {
        public static void ReadValues(ReadOnlySpan<byte> line, Encoding encoding, string separator, NewValue newValue)
        {
            ReadValues(line, encoding, encoding.GetSeparatorBuffer(separator), newValue);
        }

        public static unsafe void ReadValues(byte* start, int length, Encoding encoding, string separator, NewValue newValue)
        {
            ReadValues(start, length, encoding, encoding.GetSeparatorBuffer(separator), newValue);
        }

        private static unsafe void ReadValues(byte* start, int length, Encoding encoding, byte[] separator, NewValue newValue)
        {
            ReadValues(new ReadOnlySpan<byte>(start, length), encoding, separator, newValue);
        }

        private static void ReadValues(ReadOnlySpan<byte> line, Encoding encoding, byte[] separator, NewValue newValue)
        {
            var position = 0;
            while (true)
            {
                var next = line.IndexOf(separator);
                if (next == -1)
                {
                    newValue?.Invoke(position, line, encoding);
                    break;
                }
                newValue?.Invoke(position, line.Slice(0, next), encoding);
                line = line.Slice(next + separator.Length);
                position += next + separator.Length;
            }
        }
    }
}

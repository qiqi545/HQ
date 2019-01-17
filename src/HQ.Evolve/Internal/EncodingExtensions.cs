using System.Collections.Generic;
using System.Text;

namespace HQ.Evolve.Internal
{
    internal static class EncodingExtensions
    {
        // TODO: bench against e.GetString(new ReadOnlySpan<byte>(start, length));
        public static unsafe string ToString(this Encoding encoding, byte* start, int length)
        {
            var charCount = encoding.GetCharCount(start, length);
            fixed (char* chars = encoding.GetCharBuffer())
            {
                encoding.GetChars(start, length, chars, charCount);
                var value = chars[charCount - 2] == Constants.CarriageReturn
                    ? new string(chars, 0, charCount - 2)
                    : new string(chars, 0, charCount - 1);
                return value;
            }
        }

        private static readonly UTF32Encoding BigEndianUtf32 = new UTF32Encoding(true, true);

        #region Separator

        public static byte[] GetSeparatorBuffer(this Encoding encoding, string separator)
        {
            if (!WorkingSeparators.TryGetValue(encoding, out var buffers))
                 WorkingSeparators.Add(encoding, buffers = new Dictionary<string, byte[]>());

            if(!buffers.TryGetValue(separator, out var buffer))
                buffers.Add(separator, buffer = BuildSeparatorBuffer(encoding, separator));

            return buffer;
        }

        private static unsafe byte[] BuildSeparatorBuffer(Encoding encoding, string delimiter)
        {
            byte[] separator;
            fixed (char* chars = delimiter)
            {
                var byteCount = encoding.GetByteCount(chars, delimiter.Length);
                fixed (byte* buffer = separator = new byte[byteCount])
                {
                    encoding.GetBytes(chars, delimiter.Length, buffer, byteCount);
                }
            }

            return separator;
        }

        private static readonly Dictionary<Encoding, Dictionary<string, byte[]>> WorkingSeparators =
            new Dictionary<Encoding, Dictionary<string, byte[]>>
        {
            { Encoding.UTF7, BuildSeparatorBuffers(Encoding.UTF7) },
            { Encoding.UTF8, BuildSeparatorBuffers(Encoding.UTF8) },
            { Encoding.Unicode, BuildSeparatorBuffers(Encoding.Unicode) },
            { Encoding.BigEndianUnicode, BuildSeparatorBuffers(Encoding.BigEndianUnicode) },
            { Encoding.UTF32, BuildSeparatorBuffers(Encoding.UTF32) },
            { BigEndianUtf32, BuildSeparatorBuffers(BigEndianUtf32) },
        };

        private static Dictionary<string, byte[]> BuildSeparatorBuffers(Encoding encoding)
        {
            return new Dictionary<string, byte[]>
            {
                {Constants.Comma, BuildSeparatorBuffer(encoding, Constants.Comma)},
                {Constants.Tab, BuildSeparatorBuffer(encoding, Constants.Tab)},
                {Constants.Pipe, BuildSeparatorBuffer(encoding, Constants.Pipe)}
            };
        }

        #endregion

        #region Preamble

        public static byte[] GetPreambleBuffer(this Encoding encoding)
        {
            if (!WorkingPreambles.TryGetValue(encoding, out var buffer))
                 WorkingPreambles.Add(encoding, buffer = encoding.GetPreamble());
            return buffer;
        }

        private static readonly Dictionary<Encoding, byte[]> WorkingPreambles = new Dictionary<Encoding, byte[]>
        {
            {Encoding.UTF7, Encoding.UTF7.GetPreamble()},
            {Encoding.UTF8, Encoding.UTF8.GetPreamble()},
            {Encoding.Unicode, Encoding.Unicode.GetPreamble()},
            {Encoding.BigEndianUnicode, Encoding.BigEndianUnicode.GetPreamble()},
            {Encoding.UTF32, Encoding.UTF32.GetPreamble()},
            {BigEndianUtf32, Encoding.UTF32.GetPreamble()}
        };

        #endregion

        #region CharBuffer

        public static char[] GetCharBuffer(this Encoding encoding)
        {
            if (!WorkingChars.TryGetValue(encoding, out var buffer))
                 WorkingChars.Add(encoding, buffer = new char[encoding.GetMaxCharCount(Constants.WorkingBytesLength)]);
            return buffer;
        }

        private static readonly Dictionary<Encoding, char[]> WorkingChars = new Dictionary<Encoding, char[]>
        {
            {Encoding.UTF7, new char[Encoding.UTF7.GetMaxCharCount(Constants.WorkingBytesLength)]},
            {Encoding.UTF8, new char[Encoding.UTF8.GetMaxCharCount(Constants.WorkingBytesLength)]},
            {Encoding.Unicode, new char[Encoding.Unicode.GetMaxCharCount(Constants.WorkingBytesLength)]},
            {Encoding.BigEndianUnicode, new char[Encoding.BigEndianUnicode.GetMaxCharCount(Constants.WorkingBytesLength)]},
            {Encoding.UTF32, new char[Encoding.UTF32.GetMaxCharCount(Constants.WorkingBytesLength)]},
            {BigEndianUtf32, new char[Encoding.UTF32.GetMaxCharCount(Constants.WorkingBytesLength)]}
        };

        #endregion
    }
}

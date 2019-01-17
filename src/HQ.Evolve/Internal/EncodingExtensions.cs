#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Collections.Generic;
using System.Text;

namespace HQ.Evolve.Internal
{
    internal static class EncodingExtensions
    {
        private static readonly UTF32Encoding BigEndianUtf32 = new UTF32Encoding(true, true);

        #region Separator

        public static byte[] GetSeparatorBuffer(this Encoding encoding, string separator)
        {
            if (!WorkingSeparators.TryGetValue(encoding, out var buffers))
                WorkingSeparators.Add(encoding, buffers = new Dictionary<string, byte[]>());

            if (!buffers.TryGetValue(separator, out var buffer))
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
                {Encoding.UTF7, BuildSeparatorBuffers(Encoding.UTF7)},
                {Encoding.UTF8, BuildSeparatorBuffers(Encoding.UTF8)},
                {Encoding.Unicode, BuildSeparatorBuffers(Encoding.Unicode)},
                {Encoding.BigEndianUnicode, BuildSeparatorBuffers(Encoding.BigEndianUnicode)},
                {Encoding.UTF32, BuildSeparatorBuffers(Encoding.UTF32)},
                {BigEndianUtf32, BuildSeparatorBuffers(BigEndianUtf32)}
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
            {
                Encoding.BigEndianUnicode,
                new char[Encoding.BigEndianUnicode.GetMaxCharCount(Constants.WorkingBytesLength)]
            },
            {Encoding.UTF32, new char[Encoding.UTF32.GetMaxCharCount(Constants.WorkingBytesLength)]},
            {BigEndianUtf32, new char[Encoding.UTF32.GetMaxCharCount(Constants.WorkingBytesLength)]}
        };

        #endregion
    }
}

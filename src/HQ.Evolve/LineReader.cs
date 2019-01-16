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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using HQ.Strings;

namespace HQ.Evolve
{
    public class LineReader
    {
        private const byte CarriageReturn = (byte) '\r';
        private const byte LineFeed = (byte) '\n';

        private const int ReadAheadSize = 128;
        private const int BlockSize = 4096;
        private const int PadSize = 4;

        private static readonly byte[] WorkingBytes = new byte[ReadAheadSize + BlockSize + PadSize];

        private static readonly Dictionary<Encoding, char[]> WorkingChars = new Dictionary<Encoding, char[]>
        {
            {Encoding.UTF7, new char[Encoding.UTF7.GetMaxCharCount(WorkingBytes.Length)]},
            {Encoding.UTF8, new char[Encoding.UTF8.GetMaxCharCount(WorkingBytes.Length)]},
            {Encoding.Unicode, new char[Encoding.Unicode.GetMaxCharCount(WorkingBytes.Length)]},
            {Encoding.BigEndianUnicode, new char[Encoding.BigEndianUnicode.GetMaxCharCount(WorkingBytes.Length)]},
            {Encoding.UTF32, new char[Encoding.UTF32.GetMaxCharCount(WorkingBytes.Length)]}
        };

        private static readonly Dictionary<Encoding, byte[]> Preambles = new Dictionary<Encoding, byte[]>
        {
            {Encoding.UTF7, Encoding.UTF7.GetPreamble()},
            {Encoding.UTF8, Encoding.UTF8.GetPreamble()},
            {Encoding.Unicode, Encoding.Unicode.GetPreamble()},
            {Encoding.BigEndianUnicode, Encoding.BigEndianUnicode.GetPreamble()},
            {Encoding.UTF32, Encoding.UTF32.GetPreamble()},
        };

        public static ulong CountLines(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
        {
            return ReadOrCountLines(stream, encoding, null, cancellationToken);
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, Action<string> onNewLine, CancellationToken cancellationToken = default)
        {
            return ReadOrCountLines(stream, encoding, onNewLine, cancellationToken);
        }

        // Derived from MimeKit's MimeParser
        private static ulong ReadOrCountLines(Stream stream, Encoding encoding, Action<string> onNewLine, CancellationToken cancellationToken)
        {
            var count = 0UL;
            var offset = stream.CanSeek ? stream.Position : 0L;
            var from = 0;
            var to = ReadAheadSize;
            var endOfStream = false;

            if (!Preambles.TryGetValue(encoding, out var preamble))
                Preambles.Add(encoding, preamble = encoding.GetPreamble());

            char[] workingChars;
            if (onNewLine == null)
                workingChars = null;
            else
            {
                if (!WorkingChars.TryGetValue(encoding, out workingChars))
                     WorkingChars.Add(encoding, workingChars = new char[encoding.GetMaxCharCount(WorkingBytes.Length)]);
            }

            unsafe
            {
                fixed (byte* buffer = WorkingBytes)
                {
                    if (stream.CanSeek && stream.Position != offset)
                        stream.Seek(offset, SeekOrigin.Begin);

                    if (!ReadPreamble(stream, preamble, buffer, WorkingBytes, ref from, ref to, ref endOfStream,
                        cancellationToken))
                        throw new FormatException(ErrorStrings.Evolve_UnexpectedEndOfStream);

                    do
                    {
                        if (ReadAhead(stream, WorkingBytes, ReadAheadSize, 2, ref from, ref to, ref endOfStream,
                                cancellationToken) <= 0)
                            break;

                        var position = buffer + from;
                        var end = buffer + to;
                        var startIndex = from;

                        *end = LineFeed;

                        while (position < end)
                        {
                            var alignment = (startIndex + 3) & ~3;
                            var aligned = buffer + alignment;
                            var start = position;
                            var c = *aligned;

                            *aligned = LineFeed;
                            while (*position != LineFeed)
                                position++;
                            *aligned = c;

                            if (position == aligned && c != LineFeed)
                            {
                                var dword = (uint*) position;
                                uint mask;
                                do
                                {
                                    mask = *dword++ ^ 0x0A0A0A0A;
                                    mask = (mask - 0x01010101) & ~mask & 0x80808080;
                                } while (mask == 0);

                                position = (byte*) (dword - 1);
                                while (*position != LineFeed)
                                    position++;
                            }

                            var length = (int) (position - start);

                            if (position < end)
                            {
                                length++;
                                position++;
                                count++;
                                if (onNewLine != null)
                                    OnNewLine(encoding, onNewLine, start, length, workingChars);
                            }

                            startIndex += length;
                        }

                        from = startIndex;
                    } while (true);
                }
            }

            return count;
        }

        private static unsafe void OnNewLine(Encoding encoding, Action<string> onNewLine, byte* start, int length, char[] workingChars)
        {
            var charCount = encoding.GetCharCount(start, length);
            fixed (char* chars = workingChars)
            {
                encoding.GetChars(start, length, chars, charCount);
                var line = chars[charCount - 2] == CarriageReturn
                    ? new string(chars, 0, charCount - 2)
                    : new string(chars, 0, charCount - 1);
                onNewLine.Invoke(line);
            }
        }

        #region BOM

        // Derived from MimeKit's MimeParser
        private static unsafe bool ReadPreamble(Stream stream, ReadOnlySpan<byte> preamble, byte* buffer, byte[] workingBytes, ref int from, ref int to, ref bool eos, CancellationToken cancellationToken)
        {
            var i = 0;
            do
            {
                var available = ReadAhead(stream, workingBytes, ReadAheadSize, 0, ref from, ref to, ref eos,
                    cancellationToken);
                if (available <= 0)
                {
                    from = to;
                    return false;
                }

                var position = buffer + from;
                var end = buffer + to;
                while (position < end && i < preamble.Length && *position == preamble[i])
                {
                    i++;
                    position++;
                }

                from = (int)(position - buffer);

            } while (from == to);

            return i == 0 || i == preamble.Length;
        }

        #endregion

        #region Alignment

        // Derived from MimeKit's MimeParser
        private static int ReadAhead(Stream stream, byte[] workingBytes, int min, int save, ref int from, ref int to, ref bool endOfStream, CancellationToken cancellationToken)
        {
            if (!AlignReadAheadBuffer(workingBytes, min, save, ref from, ref to, ref endOfStream, out var remaining, out var start, out var end))
                return remaining;

            cancellationToken.ThrowIfCancellationRequested();
            var read = stream.Read(workingBytes, start, end - start);
            if (read > 0)
            {
                to += read;
            }
            else
            {
                endOfStream = true;
            }

            return to - from;
        }

        // Derived from MimeKit's MimeParser
        private static bool AlignReadAheadBuffer(byte[] workingBytes, int min, int save, ref int from, ref int to, ref bool endOfStream, out int remaining, out int start, out int end)
        {
            remaining = to - from;
            start = ReadAheadSize;
            end = to;

            if (remaining >= min || endOfStream)
                return false;

            remaining += save;

            if (remaining > 0)
            {
                var index = from - save;

                if (index >= start)
                {
                    start -= Math.Min(ReadAheadSize, remaining);
                    Buffer.BlockCopy(workingBytes, index, workingBytes, start, remaining);
                    index = start;
                    start += remaining;
                }
                else if (index > 0)
                {
                    var shift = Math.Min(index, end - start);
                    Buffer.BlockCopy(workingBytes, index, workingBytes, index - shift, remaining);
                    index -= shift;
                    start = index + remaining;
                }
                else
                {
                    start = end;
                }

                from = index + save;
                to = start;
            }
            else
            {
                from = start;
                to = start;
            }

            end = workingBytes.Length - PadSize;
            return true;
        }

        #endregion
    }
}

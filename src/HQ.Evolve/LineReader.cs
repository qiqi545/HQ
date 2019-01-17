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
using System.IO;
using System.Text;
using System.Threading;
using HQ.Evolve.Internal;
using HQ.Strings;

namespace HQ.Evolve
{
    public static class LineReader
    {
        // Derived from MimeKit's MimeParser
        private static ulong ReadOrCountLines(Stream stream, Encoding encoding, NewLine onNewLine,
            CancellationToken cancellationToken)
        {
            var count = 0UL;
            var offset = stream.CanSeek ? stream.Position : 0L;
            var from = 0;
            var to = Constants.ReadAheadSize;
            var endOfStream = false;

            var preamble = encoding.GetPreambleBuffer();

            unsafe
            {
                var workingBytes = Constants.WorkingBytes;

                fixed (byte* buffer = workingBytes)
                {
                    if (stream.CanSeek && stream.Position != offset)
                        stream.Seek(offset, SeekOrigin.Begin);

                    if (!ReadPreamble(stream, preamble, buffer, workingBytes, ref from, ref to, ref endOfStream,
                        cancellationToken))
                        throw new FormatException(ErrorStrings.Evolve_UnexpectedEndOfStream);

                    do
                    {
                        if (ReadAhead(stream, workingBytes, Constants.ReadAheadSize, 2, ref from, ref to,
                                ref endOfStream,
                                cancellationToken) <= 0)
                            break;

                        var position = buffer + from;
                        var end = buffer + to;
                        var startIndex = from;

                        *end = Constants.LineFeed;

                        while (position < end)
                        {
                            var alignment = (startIndex + 3) & ~3;
                            var aligned = buffer + alignment;
                            var start = position;
                            var c = *aligned;

                            *aligned = Constants.LineFeed;
                            while (*position != Constants.LineFeed)
                                position++;
                            *aligned = c;

                            if (position == aligned && c != Constants.LineFeed)
                            {
                                var dword = (uint*) position;
                                uint mask;
                                do
                                {
                                    mask = *dword++ ^ 0x0A0A0A0A;
                                    mask = (mask - 0x01010101) & ~mask & 0x80808080;
                                } while (mask == 0);

                                position = (byte*) (dword - 1);
                                while (*position != Constants.LineFeed)
                                    position++;
                            }

                            var length = (int) (position - start);

                            if (position < end)
                            {
                                length++;
                                position++;
                                count++;

                                onNewLine?.Invoke(count, start, length, encoding);
                            }

                            startIndex += length;
                        }

                        from = startIndex;
                    } while (true);
                }
            }

            return count;
        }

        #region BOM

        // Derived from MimeKit's MimeParser
        private static unsafe bool ReadPreamble(Stream stream, ReadOnlySpan<byte> preamble, byte* buffer,
            byte[] workingBytes, ref int from, ref int to, ref bool eos, CancellationToken cancellationToken)
        {
            var i = 0;
            do
            {
                var available = ReadAhead(stream, workingBytes, Constants.ReadAheadSize, 0, ref from, ref to, ref eos,
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

                from = (int) (position - buffer);
            } while (from == to);

            return i == 0 || i == preamble.Length;
        }

        #endregion

        #region API

        public static ulong CountLines(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
        {
            return ReadOrCountLines(stream, encoding, null, cancellationToken);
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, NewLineAsString onNewLine,
            CancellationToken cancellationToken = default)
        {
            unsafe
            {
                NewLine newLine = (n, s, l, e) => { onNewLine?.Invoke(n, e.GetString(s, l)); };
                return ReadOrCountLines(stream, encoding, newLine, cancellationToken);
            }
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, NewLine onNewLine,
            CancellationToken cancellationToken = default)
        {
            return ReadOrCountLines(stream, encoding, onNewLine, cancellationToken);
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, string separator, NewValue onNewValue,
            CancellationToken cancellationToken = default)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, start, length, e) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue);
                };
                return ReadLines(stream, encoding, onNewLine, cancellationToken);
            }
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, string separator, NewValueAsSpan onNewValue,
            CancellationToken cancellationToken = default)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, start, length, e) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue);
                };
                return ReadLines(stream, encoding, onNewLine, cancellationToken);
            }
        }

        #endregion

        #region Alignment

        // Derived from MimeKit's MimeParser
        private static int ReadAhead(Stream stream, byte[] workingBytes, int min, int save, ref int from, ref int to,
            ref bool endOfStream, CancellationToken cancellationToken)
        {
            if (!AlignReadAheadBuffer(workingBytes, min, save, ref from, ref to, ref endOfStream, out var remaining,
                out var start, out var end))
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
        private static bool AlignReadAheadBuffer(byte[] workingBytes, int min, int save, ref int from, ref int to,
            ref bool endOfStream, out int remaining, out int start, out int end)
        {
            remaining = to - from;
            start = Constants.ReadAheadSize;
            end = to;

            if (remaining >= min || endOfStream)
                return false;

            remaining += save;

            if (remaining > 0)
            {
                var index = from - save;

                if (index >= start)
                {
                    start -= Math.Min(Constants.ReadAheadSize, remaining);
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

            end = workingBytes.Length - Constants.PadSize;
            return true;
        }

        #endregion
    }
}

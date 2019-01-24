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

//
// MimeParser.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HQ.Data.Streaming.Internal;
using HQ.Extensions.Metrics;
using HQ.Strings;

namespace HQ.Data.Streaming
{
    public static class LineReader
    {
        // Derived from MimeKit's MimeParser
        private static ulong ReadOrCountLines(Stream stream, Encoding encoding, byte[] workingBuffer, NewLine onNewLine,
            CancellationToken cancellationToken, IMetricsHost metrics)
        {
            var count = 0UL;
            var offset = stream.CanSeek ? stream.Position : 0L;
            var from = Constants.ReadAheadSize;
            var to = Constants.ReadAheadSize;
            var endOfStream = false;

            var preamble = encoding.GetPreambleBuffer();

            unsafe
            {
                fixed (byte* buffer = workingBuffer)
                {
                    if (stream.CanSeek && stream.Position != offset)
                        stream.Seek(offset, SeekOrigin.Begin);

                    if (!ReadPreamble(stream, preamble, buffer, workingBuffer, ref from, ref to, ref endOfStream, cancellationToken))
                        throw new FormatException(ErrorStrings.Evolve_UnexpectedEndOfStream);

                    do
                    {
                        if (ReadAhead(stream, workingBuffer, Constants.ReadAheadSize, 2, ref from, ref to,
                                ref endOfStream,
                                cancellationToken) <= 0)
                            break;

                        var position = buffer + from;
                        var end = buffer + to;
                        var startIndex = from;

                        *end = (byte)'\n';

                        while (position < end)
                        {
                            var alignment = (startIndex + 3) & ~3;
                            var aligned = buffer + alignment;
                            var start = position;
                            var c = *aligned;

                            *aligned = (byte)'\n';
                            while (*position != (byte)'\n')
                                position++;
                            *aligned = c;

                            if (position == aligned && c != (byte)'\n')
                            {
                                var dword = (uint*) position;
                                uint mask;
                                do
                                {
                                    mask = *dword++ ^ 0x0A0A0A0A;
                                    mask = (mask - 0x01010101) & ~mask & 0x80808080;
                                } while (mask == 0);

                                position = (byte*) (dword - 1);
                                while (*position != (byte)'\n')
                                    position++;
                            }

                            var length = (int) (position - start);

                            if (position < end)
                            {
                                length++;
                                position++;
                                count++;

//#if DEBUG
//                                var span = new ReadOnlySpan<byte>(start, length);
//                                var debug = encoding.GetString(span);
//#endif
                                onNewLine?.Invoke(count, start, length, encoding, metrics);
                            }
                            else if (count == 0 && position == end)
                            {
//#if DEBUG
//                                var span = new ReadOnlySpan<byte>(start, length);
//                                var debug = encoding.GetString(span);
//#endif
                                onNewLine?.Invoke(count, start, length, encoding, metrics);
                                return 1;
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

        public static ulong CountLines(Stream stream, Encoding encoding, CancellationToken cancellationToken = default,
            IMetricsHost metrics = null)
        {
            return ReadOrCountLines(stream, encoding, Constants.WorkingBuffer, null, cancellationToken, metrics);
        }

        public static ulong CountLines(Stream stream, Encoding encoding, byte[] workingBuffer, CancellationToken cancellationToken = default,
            IMetricsHost metrics = null)
        {
            return ReadOrCountLines(stream, encoding, workingBuffer, null, cancellationToken, metrics);
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, NewLineAsString onNewLine,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            unsafe
            {
                NewLine newLine = (n, s, l, e, m) => { onNewLine?.Invoke(n, e.GetString(s, l), m); };
                return ReadOrCountLines(stream, encoding, Constants.WorkingBuffer, newLine, cancellationToken, metrics);
            }
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, NewLineAsString onNewLine,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            unsafe
            {
                NewLine newLine = (n, s, l, e, m) => { onNewLine?.Invoke(n, e.GetString(s, l), m); };
                return ReadOrCountLines(stream, encoding, workingBuffer, newLine, cancellationToken, metrics);
            }
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, NewLine onNewLine,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            return ReadOrCountLines(stream, encoding, Constants.WorkingBuffer, onNewLine, cancellationToken, metrics);
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, NewLine onNewLine,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            return ReadOrCountLines(stream, encoding, workingBuffer, onNewLine, cancellationToken, metrics);
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, string separator, NewValue onNewValue,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, start, length, e, m) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue, m);
                };
                return ReadLines(stream, encoding, onNewLine, cancellationToken);
            }
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, string separator, NewValue onNewValue,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, start, length, e, m) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue, m);
                };
                return ReadLines(stream, encoding, workingBuffer, onNewLine, cancellationToken);
            }
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, string separator, NewValueAsSpan onNewValue,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, start, length, e, m) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue, m);
                };
                return ReadLines(stream, encoding, onNewLine, cancellationToken, metrics);
            }
        }

        public static ulong ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, string separator, NewValueAsSpan onNewValue,
            CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, start, length, e, m) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue, m);
                };
                return ReadLines(stream, encoding, workingBuffer, onNewLine, cancellationToken, metrics);
            }
        }

        public static IEnumerable<LineConstructor> ReadLines(Stream stream, Encoding encoding,
            string separator, CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            return ReadLines(stream, encoding, Constants.WorkingBuffer, encoding.GetSeparatorBuffer(separator ?? Environment.NewLine),
                cancellationToken, metrics);
        }

        public static IEnumerable<LineConstructor> ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer,
            string separator, CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            return ReadLines(stream, encoding, workingBuffer, encoding.GetSeparatorBuffer(separator ?? Environment.NewLine),
                cancellationToken, metrics);
        }
        
        public static IEnumerable<LineConstructor> ReadLines(Stream stream, Encoding encoding,
            byte[] separator, CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            return ReadLines(stream, encoding, Constants.WorkingBuffer, separator, cancellationToken, metrics);
        }

        public static IEnumerable<LineConstructor> ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer,
            byte[] separator, CancellationToken cancellationToken = default, IMetricsHost metrics = null)
        {
            var queue = new BlockingCollection<LineConstructor>(new ConcurrentQueue<LineConstructor>());

            void ReadLines(Encoding e)
            {
                try
                {
                    unsafe
                    {
                        LineReader.ReadLines(stream, e, workingBuffer, (lineNumber, start, length, x, m) =>
                        {
                            queue.Add(new LineConstructor
                            {
                                lineNumber = lineNumber,
                                start = start,
                                length = length
                            }, cancellationToken);
                        }, cancellationToken, metrics);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    queue.CompleteAdding();
                }
            }

            Task.Run(() => ReadLines(encoding), cancellationToken);

            return queue.GetConsumingEnumerable();
        }

        #endregion

        #region Alignment

        // Derived from MimeKit's MimeParser
        private static int ReadAhead(Stream stream, byte[] workingBytes, int minimumSize, int save, ref int from, ref int to,
            ref bool endOfStream, CancellationToken cancellationToken)
        {
            if (!AlignReadAheadBuffer(workingBytes, minimumSize, save, ref from, ref to, ref endOfStream, out var remaining, out var start, out var end))
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

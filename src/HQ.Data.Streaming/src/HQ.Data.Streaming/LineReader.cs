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
        private static long ReadOrCountLines(Stream stream, Encoding encoding, byte[] workingBuffer, NewLine onNewLine,
            CancellationToken cancellationToken, IMetricsHost metrics)
        {
            var count = 0L;
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

                                BytesPerSecond(metrics, length);
                                onNewLine?.Invoke(count, false, start, length, encoding, metrics);
                            }
                            else if (count == 0 && position == end)
                            {
                                BytesPerSecond(metrics, length);
                                onNewLine?.Invoke(count, false, start, length, encoding, metrics);
                                return 1;
                            }
                            else
                            {
                                // line spans across the read-ahead buffer
                                BytesPerSecond(metrics, length);
                                onNewLine?.Invoke(count, true, start, length, encoding, metrics);
                            }

                            startIndex += length;
                        }

                        from = startIndex;
                    } while (true);
                }
            }

            return count;
        }

        private static void BytesPerSecond(IMetricsHost metrics, int length)
        {
            metrics?.Meter(typeof(LineReader), "bytes_read_per_second", "bytes", TimeUnit.Seconds).Mark(length);
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

        public static long CountLines(Stream stream, Encoding encoding, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            return CountLines(stream, encoding, Constants.Buffer, metrics, cancellationToken);
        }

        public static long CountLines(Stream stream, Encoding encoding, byte[] workingBuffer, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            return ReadOrCountLines(stream, encoding, workingBuffer, null, cancellationToken, metrics);
        }
        
        public static long ReadLines(Stream stream, Encoding encoding, NewLineAsString onNewLine,
            IMetricsHost metrics = null,
            CancellationToken cancellationToken = default)
        {
            unsafe
            {
                var pendingLength = 0;
                byte[] buffer = null; // TODO convert to allocator

                NewLine newLine = (lineNumber, partial, start, length, x, m) =>
                {
                    if (buffer == null)
                        buffer = new byte[Constants.ReadAheadSize * 2];

                    var target = new Span<byte>(buffer, pendingLength, length);
                    var segment = new ReadOnlySpan<byte>(start, length);
                    segment.CopyTo(target);

                    if (partial)
                    {
                        pendingLength = length;
                    }
                    else
                    {
                        var line = new ReadOnlySpan<byte>(buffer, 0, length + pendingLength);
                        onNewLine?.Invoke(lineNumber, encoding.GetString(line), m);
                        pendingLength = 0;
                        buffer = null;
                    }
                };
                return ReadOrCountLines(stream, encoding, Constants.Buffer, newLine, cancellationToken, metrics);
            }
        }

        public static long ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, NewLineAsString onNewLine,
            IMetricsHost metrics = null,
            CancellationToken cancellationToken = default)
        {
            unsafe
            {
                NewLine newLine = (n, p, s, l, e, m) => { onNewLine?.Invoke(n, e.GetString(s, l), m); };
                return ReadOrCountLines(stream, encoding, workingBuffer, newLine, cancellationToken, metrics);
            }
        }

        public static long ReadLines(Stream stream, Encoding encoding, NewLine onNewLine,
            IMetricsHost metrics = null,
            CancellationToken cancellationToken = default)
        {
            return ReadOrCountLines(stream, encoding, Constants.Buffer, onNewLine, cancellationToken, metrics);
        }

        public static long ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, NewLine onNewLine, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            return ReadOrCountLines(stream, encoding, workingBuffer, onNewLine, cancellationToken, metrics);
        }

        public static long ReadLines(Stream stream, Encoding encoding, string separator, NewValue onNewValue, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            return ReadLines(stream, encoding, Constants.Buffer, separator, onNewValue, metrics, cancellationToken);
        }

        public static long ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, string separator, NewValue onNewValue, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, partial, start, length, e, m) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue, m);
                };
                return ReadLines(stream, encoding, workingBuffer, onNewLine, metrics, cancellationToken);
            }
        }

        public static long ReadLines(Stream stream, Encoding encoding, string separator, NewValueAsSpan onNewValue, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            return ReadLines(stream, encoding, Constants.Buffer, separator, onNewValue, metrics, cancellationToken);
        }

        public static long ReadLines(Stream stream, Encoding encoding, byte[] workingBuffer, string separator, NewValueAsSpan onNewValue, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            unsafe
            {
                NewLine onNewLine = (lineNumber, partial, start, length, e, m) =>
                {
                    LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue, m);
                };
                return ReadLines(stream, encoding, workingBuffer, onNewLine, metrics, cancellationToken);
            }
        }

        public static IEnumerable<LineConstructor> StreamLines(Stream stream, Encoding encoding,
            string separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null,
            CancellationToken cancellationToken = default)
        {
            return StreamLines(stream, encoding, Constants.Buffer, encoding.GetSeparatorBuffer(separator ?? Environment.NewLine), maxWorkingMemoryBytes, metrics, cancellationToken);
        }

        public static IEnumerable<LineConstructor> StreamLines(Stream stream, Encoding encoding, byte[] workingBuffer,
            string separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null,
            CancellationToken cancellationToken = default)
        {
            return StreamLines(stream, encoding, workingBuffer, encoding.GetSeparatorBuffer(separator ?? Environment.NewLine), maxWorkingMemoryBytes, metrics, cancellationToken);
        }
        
        public static IEnumerable<LineConstructor> StreamLines(Stream stream, Encoding encoding,
            byte[] separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null,
            CancellationToken cancellationToken = default)
        {
            return StreamLines(stream, encoding, Constants.Buffer, separator, maxWorkingMemoryBytes, metrics, cancellationToken);
        }

        public static IEnumerable<LineConstructor> StreamLines(Stream stream, Encoding encoding, byte[] workingBuffer,
            byte[] separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null,
            CancellationToken cancellationToken = default)
        {
            var queue = new BlockingCollection<LineConstructor>(new ConcurrentQueue<LineConstructor>());
           
            void ReadLines(Encoding e) 
            {
                var pendingLength = 0;
                byte[] buffer = null; // TODO convert to allocator

                try
                {
                    unsafe
                    {
                        LineReader.ReadLines(stream, e, workingBuffer, (lineNumber, partial, start, length, x, m) =>
                        {
                            if (buffer == null)
                                buffer = new byte[Math.Max(length, Constants.ReadAheadSize * 2)];

                            var target = new Span<byte>(buffer, pendingLength, length);
                            var segment = new ReadOnlySpan<byte>(start, length);
                            segment.CopyTo(target);

                            if (partial)
                            {
                                pendingLength = length;
                            }
                            else
                            {
                                var ctor = new LineConstructor{lineNumber = lineNumber, length = length + pendingLength, buffer = buffer};

                                if(maxWorkingMemoryBytes > 0)
                                {
                                    var usedBytes = queue.Count * (buffer.Length + sizeof(long) + sizeof(int));
                                    while (usedBytes > maxWorkingMemoryBytes)
                                        Task.Delay(10, cancellationToken).Wait(cancellationToken);
                                }

                                queue.Add(ctor, cancellationToken);
                                pendingLength = 0;
                                buffer = null;
                            }

                        }, metrics, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    throw;
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

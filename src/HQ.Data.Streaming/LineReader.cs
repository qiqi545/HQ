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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HQ.Data.Streaming.Internal;
using HQ.Extensions.Logging;
using HQ.Extensions.Metrics;
using TypeKitchen;

using Constants = HQ.Data.Streaming.Internal.Constants;

namespace HQ.Data.Streaming
{
	public static class LineReader
	{
		public static string GetHeaderText<TMetadata>(string separator)
		{
			return Pooling.StringBuilderPool.Scoped(sb =>
			{
				var members = AccessorMembers.Create(typeof(TMetadata), AccessorMemberTypes.Fields,
					AccessorMemberScope.Public);

				// FIXME: convert to zero-alloc
				var columns = members
					.Where(x => x.HasAttribute<ColumnAttribute>())
					.OrderBy(x => x.TryGetAttribute(out ColumnAttribute column) ? -1 : column.Order)
					.ToArray();

				var i = 0;
				foreach (var member in columns)
				{
					member.TryGetAttribute(out ColumnAttribute column);
					member.TryGetAttribute(out DisplayAttribute display);
					var name = display?.Name ?? column.Name ?? member.Name;
					sb.Append(name);

					i++;
					if (i < columns.Length)
						sb.Append(separator);
				}
			});
		}
		
		#region BOM

		// Derived from MimeKit's MimeParser
		private static unsafe bool ReadPreamble(Stream stream, ReadOnlySpan<byte> preamble, byte* buffer, byte[] workingBytes, ref int from, ref int to, ref bool eos, CancellationToken cancellationToken)
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

		public static long CountLines(Stream stream, Encoding encoding, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => CountLines(stream, encoding, Constants.Buffer, logger, metrics, cancellationToken);
		public static long CountLines(Stream stream, Encoding encoding, byte[] workingBuffer, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => ReadOrCountLines(stream, encoding, workingBuffer, null, logger, metrics, cancellationToken);

		public static long ReadLines(Stream stream, Encoding encoding, NewLineAsString onNewLine, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			unsafe
			{
				var pendingLength = 0;
				var buffer = Constants.Buffer;

				return ReadOrCountLines(stream, encoding, buffer, OnNewLine, logger, metrics, cancellationToken);

				void OnNewLine(long lineNumber, bool partial, byte* start, int length, Encoding x)
				{
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
						onNewLine?.Invoke(lineNumber, encoding.GetString(line));
						pendingLength = 0;
					}
				}
			}
		}
		public static long ReadLines(Stream stream, Encoding encoding, byte[] buffer, NewLineAsString onNewLine, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			unsafe
			{
				void NewLine(long n, bool p, byte* s, int l, Encoding e)
				{
					onNewLine?.Invoke(n, e.GetString(s, l));
				}

				return ReadOrCountLines(stream, encoding, buffer, NewLine, logger, metrics, cancellationToken);
			}
		}

		public static long ReadLines(Stream stream, Encoding encoding, NewLine onNewLine, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => ReadOrCountLines(stream, encoding, Constants.Buffer, onNewLine, logger, metrics, cancellationToken);
		public static long ReadLines(Stream stream, Encoding encoding, byte[] buffer, NewLine onNewLine, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => ReadOrCountLines(stream, encoding, buffer, onNewLine, logger, metrics, cancellationToken);
		public static long ReadLines(Stream stream, Encoding encoding, string separator, NewValue onNewValue, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => ReadLines(stream, encoding, Constants.Buffer, separator, onNewValue, logger, metrics, cancellationToken);
		public static long ReadLines(Stream stream, Encoding encoding, byte[] buffer, string separator, NewValue onNewValue, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) 
		{
			unsafe
			{
				void OnNewLine(long lineNumber, bool partial, byte* start, int length, Encoding e)
				{
					LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue);
				}

				return ReadLines(stream, encoding, buffer, OnNewLine, logger, metrics, cancellationToken);
			}
		}

		public static long ReadLines(Stream stream, Encoding encoding, string separator, NewValueAsSpan onNewValue, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => ReadLines(stream, encoding, Constants.Buffer, separator, onNewValue, logger, metrics, cancellationToken);
		public static long ReadLines(Stream stream, Encoding encoding, byte[] buffer, string separator, NewValueAsSpan onNewValue, ISafeLogger logger = null,IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			unsafe
			{
				return ReadLines(stream, encoding, buffer, OnNewLine, logger, metrics, cancellationToken);

				void OnNewLine(long lineNumber, bool partial, byte* start, int length, Encoding e)
				{
					LineValuesReader.ReadValues(lineNumber, start, length, e, separator, onNewValue);
				}
			}
		}

		private static readonly ConcurrentDictionary<int, byte[]> PendingStreams = new ConcurrentDictionary<int, byte[]>();

		public static IEnumerable<LineConstructor> StreamLines(Stream stream, Encoding encoding, int maxWorkingMemoryBytes = 0, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => StreamLines(stream, encoding, Constants.Buffer, maxWorkingMemoryBytes, logger, metrics, cancellationToken);
		public static IEnumerable<LineConstructor> StreamLines(Stream stream, Encoding encoding, byte[] buffer, int maxWorkingMemoryBytes = 0, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			buffer ??= Constants.Buffer;

			var queue = new BlockingCollection<LineConstructor>(new ConcurrentQueue<LineConstructor>());

			logger?.Debug(()=> "Started streaming lines");
			
			var task = new Task(() =>
			{
				if (!Task.CurrentId.HasValue)
					throw new ArgumentNullException(nameof(buffer), "Could not find current thread ID");

				var id = Task.CurrentId.Value;

				long count = 0;
				var pendingLength = 0;
				try
				{
					unsafe
					{
						count = ReadLines(stream, encoding, PendingStreams[id], (lineNumber, partial, start, length, e) =>
						{
							var line = new ReadOnlySpan<byte>(start, length);
							if (partial)
							{
								pendingLength = length;
							}
							else
							{
								var memory = new byte[length];
								line.CopyTo(memory);

								var ctor = new LineConstructor
								{
									lineNumber = lineNumber, 
									length = length + pendingLength, 
									buffer = memory
								};

								if (maxWorkingMemoryBytes > 0)
								{
									var usedBytes = queue.Count * (PendingStreams[id].Length + sizeof(long) + sizeof(int));
									while (usedBytes > maxWorkingMemoryBytes) Task.Delay(10, cancellationToken).Wait(cancellationToken);
								}

								queue.Add(ctor, cancellationToken);
								pendingLength = 0;
							}
						}, logger, metrics, cancellationToken);
					}
				}
				catch (Exception ex)
				{
					logger?.Error(() => "Error streaming lines", ex);
				}
				finally
				{
					logger?.Debug(() => "Finished streaming {Count} lines", count);
					queue.CompleteAdding();

					if (!PendingStreams.TryRemove(id, out _))
						throw new InvalidOperationException("Line stream failed trying to clean up");
				}
			}, cancellationToken);

			if(!PendingStreams.TryAdd(task.Id, buffer))
				throw new InvalidOperationException("Line stream failed trying to start up");

			task.Start();

			return queue.GetConsumingEnumerable(cancellationToken);
		}

		#endregion

		#region Alignment

		// Derived from MimeKit's MimeParser
		private static long ReadOrCountLines(Stream stream, Encoding encoding, byte[] workingBuffer, NewLine onNewLine, ISafeLogger logger, IMetricsHost metrics, CancellationToken cancellationToken)
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
						throw new FormatException(ErrorStrings.UnexpectedEndOfStream);

					do
					{
						if (ReadAhead(stream, workingBuffer, Constants.ReadAheadSize, 2, ref from, ref to, ref endOfStream, cancellationToken) <= 0)
						{
							break;
						}

						var position = buffer + from;
						var end = buffer + to;
						var startIndex = from;

						*end = (byte) '\n';

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

							BuiltInMetrics.BytesPerSecond(metrics, length);

							if (position < end)
							{
								length++;
								position++;
								count++;

								onNewLine?.Invoke(count, false, start, length, encoding);
							}
							else if (count == 0 && position == end)
							{
								onNewLine?.Invoke(count, false, start, length, encoding);
								return 1;
							}
							else
							{
								// line spans across the read-ahead buffer
								onNewLine?.Invoke(count, true, start, length, encoding);
							}

							startIndex += length;
						}

						from = startIndex;
					} while (true);
				}
			}

			return count;
		}
		
		// Derived from MimeKit's MimeParser
		private static int ReadAhead(Stream stream, byte[] workingBytes, int minimumSize, int save, ref int from, ref int to, ref bool endOfStream, CancellationToken cancellationToken)
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
		private static bool AlignReadAheadBuffer(byte[] workingBytes, int min, int save, ref int from, ref int to, ref bool endOfStream, out int remaining, out int start, out int end)
		{
			remaining = to - from;
			start = Constants.ReadAheadSize;
			end = to;

			if (remaining >= min || endOfStream)
			{
				return false;
			}

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
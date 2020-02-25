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
using System.Linq;
using System.Text;
using System.Threading;
using ActiveLogging;
using HQ.Data.Streaming.Internal;
using Metrics;

namespace HQ.Data.Streaming.Memory
{
	public class FileMemoryProvider<T> : IExternalMemoryProvider<T>
	{
		private readonly string _baseDirectory;
		private readonly int _hashCode;

		private readonly Func<string, T> _deserialize;
		private readonly Func<T, string> _serialize;

		public FileMemoryProvider(Func<T, string> serialize, Func<string, T> deserialize, string baseDirectory)
		{
			_hashCode = Guid.NewGuid().GetHashCode();
			_serialize = serialize;
			_deserialize = deserialize;
			_baseDirectory = baseDirectory ?? Path.GetTempPath();
		}

		public Stream CreateSegment(string label, int index) => File.OpenWrite(GetFilePathForSegment(label, index));

		public void DeleteSegment(string label, int index) => File.Delete(GetFilePathForSegment(label, index));

		public IEnumerable<T> Read(string label, int index, IComparer<T> sort, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default) => Read(File.OpenRead(GetFilePathForSegment(label, index)), sort, logger, metrics, cancellationToken);
		public IEnumerable<T> Read(Stream stream, IComparer<T> sort, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			var list = new List<T>();
			LineReader.ReadLines(stream, Encoding.UTF8, (lineNumber, line) =>
			{
				var t = _deserialize(line);
				list.Add(t);
			}, logger, metrics, cancellationToken);
			list.Sort(sort);
			return list;
		}

		public IEnumerable<Stream> GetAllSegments(string label)
		{
			return Directory.GetFiles(_baseDirectory, $"{label}_{_hashCode}_*.bin")
				.OrderBy(x => x).Select(File.OpenRead);
		}

		public SegmentStats Segment(string label, IEnumerable<T> stream, int maxWorkingMemoryBytes = 0, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			
			var count = 0L;
			var segments = 0;
			var sw = new StreamWriter(CreateSegment(label, segments));
			try
			{
				foreach (var item in stream)
				{
					var line = _serialize(item);
					BuiltInMetrics.LineLength<T>(metrics, line.Length);

					sw.WriteLine(line);
					count++;

					if (maxWorkingMemoryBytes == 0 || sw.BaseStream.Length < maxWorkingMemoryBytes)
					{
						continue;
					}

					sw.Flush();
					sw.Close();
					segments++;
					sw = new StreamWriter(CreateSegment(label, segments));
				}
			}
			finally
			{
				sw.Flush();
				sw.Close();
			}

			return new SegmentStats
			{
				RecordCount = count, 
				RecordLength = (int) BuiltInMetrics.GetMeanLineLength<T>(metrics) + 1, 
				SegmentCount = segments
			};
		}

		public void Sort(string fromLabel, string toLabel, IComparer<T> sort, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			var streams = GetAllSegments(fromLabel);
			var data = new Queue<Stream>(streams);
			for (var i = 0; i < data.Count; i++)
			{
				using (var stream = data.Dequeue())
				{
					using var sw = new StreamWriter(CreateSegment(toLabel, i));

					var items = Read(stream, sort, logger, metrics, cancellationToken);
					foreach (var item in items)
					{
						var line = _serialize(item);
						sw.WriteLine(line);
					}

					sw.Flush();
				}

				DeleteSegment(fromLabel, i);
			}
		}

		/// <summary>
		///     https://en.wikipedia.org/wiki/K-way_merge_algorithm
		/// </summary>
		public IEnumerable<T> Merge(string label, SegmentStats stats, int maxWorkingMemoryBytes = 0, ISafeLogger logger = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
		{
			var segments = GetAllSegments(label).ToList();
			var queueSize = (int) Math.Min(stats.RecordCount, maxWorkingMemoryBytes / segments.Count / stats.RecordLength);
			if (queueSize == 0)
			{
				queueSize = (int) stats.RecordCount;
			}

			var readers = new StreamReader[segments.Count];
			for (var i = 0; i < segments.Count; i++)
			{
				readers[i] = new StreamReader(segments[i]);
			}

			var queues = new Queue<string>[segments.Count];
			for (var i = 0; i < segments.Count; i++)
			{
				queues[i] = new Queue<string>(queueSize);
			}

			for (var i = 0; i < segments.Count; i++)
			{
				for (var j = 0; j < queueSize; j++)
				{
					if (readers[i].Peek() < 0)
					{
						break;
					}

					var line = readers[i].ReadLine();
					queues[i].Enqueue(line);
				}
			}

			try
			{
				while (true)
				{
					var segment = -1;
					var current = string.Empty;

					for (var i = 0; i < segments.Count; i++)
					{
						var queue = queues[i];
						if (queue == default)
						{
							continue;
						}

						var next = queue.Peek();
						if (segment >= 0 && string.CompareOrdinal(next, current) >= 0)
						{
							continue;
						}

						segment = i;
						current = next;
					}

					if (segment == -1)
					{
						break;
					}

					var l = queues[segment].Dequeue();

					yield return _deserialize(l);

					if (queues[segment].Count != 0)
					{
						continue;
					}

					for (var i = 0; i < queueSize; i++)
					{
						var next = readers[segment].Peek();
						if (next < 0)
						{
							break;
						}

						var line = readers[segment].ReadLine();
						queues[segment].Enqueue(line);
					}

					if (queues[segment].Count == 0)
					{
						queues[segment] = null;
					}
				}
			}
			finally
			{
				for (var i = 0; i < segments.Count; i++)
				{
					readers[i].Close();
				}

				for (var i = 0; i < segments.Count; i++)
				{
					DeleteSegment(label, i);
				}
			}
		}

		private string GetFilePathForSegment(string label, int index)
		{
			return $"{_baseDirectory}\\{label}_{_hashCode}_{index:D3}.bin";
		}
	}
}
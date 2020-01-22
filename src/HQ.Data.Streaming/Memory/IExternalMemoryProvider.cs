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
using System.IO;
using System.Threading;
using HQ.Extensions.Metrics;

namespace HQ.Data.Streaming.Memory
{
	public interface IExternalMemoryProvider<T>
	{
		IEnumerable<Stream> GetAllSegments(string label);
		Stream CreateSegment(string label, int index);
		void DeleteSegment(string label, int index);
		IEnumerable<T> Read(string label, int index, IComparer<T> sort = null, IMetricsHost metrics = null, CancellationToken cancellationToken = default);
		IEnumerable<T> Read(Stream stream, IComparer<T> sort, IMetricsHost metrics = null, CancellationToken cancellationToken = default);
		SegmentStats Segment(string label, IEnumerable<T> stream, int maxWorkingMemoryBytes, IMetricsHost metrics = null, CancellationToken cancellationToken = default);
		void Sort(string fromLabel, string toLabel, IComparer<T> sort, IMetricsHost metrics = null,  CancellationToken cancellationToken = default);
		IEnumerable<T> Merge(string label, SegmentStats stats, int maxWorkingMemoryBytes, IMetricsHost metrics = null, CancellationToken cancellationToken = default);
	}
}
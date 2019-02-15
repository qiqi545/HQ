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
        void Sort(string fromLabel, string toLabel, IComparer<T> sort, IMetricsHost metrics = null, CancellationToken cancellationToken = default);
        IEnumerable<T> Merge(string label, int maxWorkingMemoryBytes, SegmentStats stats, IMetricsHost metrics = null, CancellationToken cancellationToken = default);
    }
}

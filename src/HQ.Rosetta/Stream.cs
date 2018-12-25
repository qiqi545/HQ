using System;
using System.Collections;
using System.Collections.Generic;

namespace HQ.Rosetta
{
    public class Stream<T> : IStream<T>
    {
        private readonly IEnumerable<T> _source;

        public Stream(IEnumerable<T> source, int count, long start, long end, string before, string after, long totalCount)
        {
            _source = source;

            Start = start;
            End = end;
            Count = count;
            BeforePage = before;
            AfterPage = after;
            TotalCount = totalCount;
        }

        public long Start { get; }
        public long End { get; }
        public int Count { get; }
        public long TotalCount { get; }
        public long TotalPages => (int)Math.Ceiling(TotalCount / (double)Count);
        public bool HasPreviousPage => !string.IsNullOrWhiteSpace(BeforePage);
        public bool HasNextPage => !string.IsNullOrWhiteSpace(AfterPage);
        public string BeforePage { get; }
        public string AfterPage { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
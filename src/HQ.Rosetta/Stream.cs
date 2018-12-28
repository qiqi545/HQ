using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HQ.Rosetta
{

    [DataContract]
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
            TotalPages = (int)Math.Ceiling(TotalCount / (double)Count);
            HasPreviousPage = !string.IsNullOrWhiteSpace(BeforePage);
            HasNextPage = !string.IsNullOrWhiteSpace(AfterPage);
        }

        [DataMember]
        public long Start { get; set; }

        [DataMember]
        public long End { get; set; }

        [DataMember]
        public int Count { get; set; }

        [DataMember]
        public long TotalCount { get; set; }

        [DataMember]
        public long TotalPages { get; set; }

        [DataMember]
        public bool HasPreviousPage { get; set; }

        [DataMember]
        public bool HasNextPage { get; set; }

        [DataMember]
        public string BeforePage { get; set; }

        [DataMember]
        public string AfterPage { get; set; }

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

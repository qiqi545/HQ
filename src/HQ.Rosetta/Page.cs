using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Rosetta
{
    public class Page<T> : IPage<T>
    {
        private readonly IEnumerable<T> _source;

        public int Index { get; }
        public int Size { get; }
        public int Count { get; }
        public long TotalCount { get; }

        public long TotalPages => (int) Math.Ceiling(TotalCount / (double) Size);
        public bool HasPreviousPage => Index > 1;
        public bool HasNextPage => Index < TotalPages;
        public int Start => Size * Index - Size + 1;
        public int End => Count == Size ? Start + Size - 1 : Start + Count - 1;

        public Page(IEnumerable<T> source, int count, int index, int size, long totalCount)
        {
            _source = source;

            Index = index;
            Size = size;
            Count = count;
            TotalCount = totalCount;
        }

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

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
using System.Collections;
using System.Collections.Generic;

namespace HQ.Rosetta
{
    public class Page<T> : IPage<T>
    {
        private readonly IEnumerable<T> _source;

        public Page(IEnumerable<T> source, int count, int index, int size, long totalCount)
        {
            _source = source;

            Index = index;
            Size = size;
            Count = count;
            TotalCount = totalCount;
        }

        public int Index { get; }
        public int Size { get; }
        public int Count { get; }
        public long TotalCount { get; }

        public long TotalPages => (int)Math.Ceiling(TotalCount / (double)Count);
        public bool HasPreviousPage => Index > 1;
        public bool HasNextPage => Index < TotalPages - 1;
        public int Start => Count * Index - Count + 1;
        public int End => Start + Count - 1;

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

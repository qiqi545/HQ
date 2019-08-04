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
using System.Runtime.Serialization;

namespace HQ.Data.Contracts
{
	[DataContract]
	public class Page : IPage
	{
		private readonly IEnumerable _source;

		public Page(IEnumerable source, int count, int index, int size, long totalCount)
		{
			_source = source;

			Index = index;
			Size = size;
			Count = count;
			TotalCount = totalCount;
			TotalPages = (int) Math.Ceiling(TotalCount / (double) Count);
			HasPreviousPage = Index > 1;
			HasNextPage = Index < TotalPages - 1;
			Start = Count * Index - Count + 1;
			End = Start + Count - 1;
		}

		[DataMember] public int Index { get; set; }

		[DataMember] public int Size { get; set; }

		[DataMember] public int Count { get; set; }

		[DataMember] public long TotalCount { get; set; }

		[DataMember] public long TotalPages { get; set; }

		[DataMember] public bool HasPreviousPage { get; set; }

		[DataMember] public bool HasNextPage { get; set; }

		[DataMember] public int Start { get; set; }

		[DataMember] public int End { get; set; }

		public IEnumerator GetEnumerator()
		{
			return _source.GetEnumerator();
		}
	}

	[DataContract]
	public class Page<T> : Page, IPage<T>
	{
		private readonly IEnumerable<T> _source;

		public Page(IEnumerable<T> source, int count, int index, int size, long totalCount) : base(source, count, index,
			size, totalCount) => _source = source;

		public new IEnumerator<T> GetEnumerator()
		{
			return _source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
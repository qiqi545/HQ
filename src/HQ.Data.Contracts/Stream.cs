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
	public class Stream<T> : IStream<T>
	{
		private readonly IEnumerable<T> _source;

		public Stream(IEnumerable<T> source, int count, long start, long end, string before, string after,
			long totalCount)
		{
			_source = source;

			Start = start;
			End = end;
			Count = count;
			BeforePage = before;
			AfterPage = after;
			TotalCount = totalCount;
			TotalPages = (int) Math.Ceiling(TotalCount / (double) Count);
			HasPreviousPage = !string.IsNullOrWhiteSpace(BeforePage);
			HasNextPage = !string.IsNullOrWhiteSpace(AfterPage);
		}

		[DataMember] public long TotalPages { get; set; }

		[DataMember] public long Start { get; set; }

		[DataMember] public long End { get; set; }

		[DataMember] public int Count { get; set; }

		[DataMember] public long TotalCount { get; set; }

		[DataMember] public bool HasPreviousPage { get; set; }

		[DataMember] public bool HasNextPage { get; set; }

		[DataMember] public string BeforePage { get; set; }

		[DataMember] public string AfterPage { get; set; }

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
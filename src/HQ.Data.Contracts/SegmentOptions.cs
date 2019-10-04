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
using System.Net;
using HQ.Data.Contracts.Configuration;

namespace HQ.Data.Contracts
{
	public class SegmentOptions : IQueryValidator
	{
		public static readonly SegmentOptions Empty = new SegmentOptions();

		public IEnumerable<long> Ids { get; set; }
		public long StartingAt { get; set; }
		public int Count { get; set; }

		public bool Validate(Type type, QueryOptions options, out IList<Error> errors)
		{
			var list = new List<Error>();
			if (StartingAt < 0 || StartingAt > Count)
				list.Add(new Error(ErrorEvents.InvalidParameter, ErrorStrings.PageRangeInvalid,
					HttpStatusCode.BadRequest));
			if (Count > options.PerPageMax)
				list.Add(new Error(ErrorEvents.InvalidParameter, ErrorStrings.PerPageTooHigh,
					HttpStatusCode.RequestEntityTooLarge));
			errors = list;
			return list.Count == 0;
		}
	}
}
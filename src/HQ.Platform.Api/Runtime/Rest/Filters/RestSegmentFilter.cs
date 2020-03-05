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
using ActiveErrors;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Runtime;
using HQ.Data.Contracts.Configuration;
using HQ.Platform.Api.Runtime.Rest.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Runtime.Rest.Filters
{
	public class RestSegmentFilter : IRestFilter
	{
		private readonly IOptions<QueryOptions> _options;

		public RestSegmentFilter(IOptions<QueryOptions> options) => _options = options;

		public QueryOptions Options => _options.Value;

		public void Execute(IDictionary<string, StringValues> qs, ref QueryContext context)
		{
			qs.TryGetValue(nameof(SegmentOptions.Ids), out var ids);
			qs.TryGetValue(nameof(SegmentOptions.StartingAt), out var startingAt);
			qs.TryGetValue(nameof(SegmentOptions.Count), out var count);

			var options = new SegmentOptions();

			var idsValue = new List<long>();
			if (ids.Count != 0)
			{
				foreach (var item in ids[0].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
				{
					if (long.TryParse(item, out var idValue))
						idsValue.Add(idValue);
				}
			}

			if (ids.Count == 0 || !long.TryParse(startingAt[0], out var startingAtValue))
				startingAtValue = 0;

			if (startingAt.Count == 0 || !int.TryParse(count[0], out var countValue))
				countValue = 0;

			options.Ids = idsValue;
			options.StartingAt = startingAtValue;
			options.Count = countValue;

			if (!options.Validate(context.Type, _options.Value, out var errors))
			{
				context.Errors.Add(new Error(ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed,
					HttpStatusCode.BadRequest, errors));
			}

			context.Buffer = options;
		}
	}
}
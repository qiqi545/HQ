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
using System.Net;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts.Runtime;
using HQ.Platform.Api.Runtime.Rest.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Runtime.Rest.Filters
{
	public class RestStreamFilter : IRestFilter
    {
        private readonly IOptions<QueryOptions> _options;

        public RestStreamFilter(IOptions<QueryOptions> options)
        {
            _options = options;
        }

        public QueryOptions Options => _options.Value;

        public void Execute(IDictionary<string, StringValues> qs, ref QueryContext context)
        {
            qs.TryGetValue(_options.Value.AfterOperator, out var after);
            qs.TryGetValue(_options.Value.BeforeOperator, out var before);

            var options = new StreamOptions();

            if (after.Count == 0 || !long.TryParse(after[0], out var afterValue))
            {
                afterValue = 0;
            }

            if (before.Count == 0 || !long.TryParse(before[0], out var beforeValue))
            {
                beforeValue = _options.Value.PerPageDefault + _options.Value.PerPageDefault * afterValue;
            }

            options.After = afterValue;
            options.Before = beforeValue;

            if (!options.Validate(context.Type, _options.Value, out var errors))
            {
                context.Errors.Add(new Error(ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed,
                    HttpStatusCode.BadRequest, errors));
            }

            context.Streaming = options;
        }
    }
}

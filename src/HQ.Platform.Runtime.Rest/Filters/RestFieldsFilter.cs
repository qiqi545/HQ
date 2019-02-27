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
using HQ.Data.Contracts;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts.Runtime;
using HQ.Platform.Runtime.Rest.Models;
using HQ.Strings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Runtime.Rest.Filters
{
    public class RestFieldsFilter : IRestFilter
    {
        private readonly IOptions<QueryOptions> _options;

        public RestFieldsFilter(IOptions<QueryOptions> options)
        {
            _options = options;
        }

        public QueryOptions Options => _options.Value;

        public void Execute(IDictionary<string, StringValues> qs, ref QueryContext context)
        {
            qs.TryGetValue(_options.Value.FieldsOperator, out var fields);

            if (fields.Count == 0)
            {
                return;
            }

            var options = new FieldOptions();
            foreach (var field in fields)
            foreach (var value in field.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
            {
                var v = value.Trim();
                options.Fields.Add(v);
            }

            if (!options.Validate(context.Type, _options.Value, out var errors))
            {
                context.Errors.Add(new Error(ErrorEvents.ValidationFailed, ErrorStrings.Adapt_ValidationFailed,
                    HttpStatusCode.BadRequest, errors));
            }

            context.Fields = options;
        }
    }
}

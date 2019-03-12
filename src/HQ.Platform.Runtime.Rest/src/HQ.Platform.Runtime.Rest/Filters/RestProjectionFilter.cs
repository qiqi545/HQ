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
using System.Net;
using FastMember;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts.Runtime;
using HQ.Platform.Runtime.Rest.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Runtime.Rest.Filters
{
    public class RestProjectionFilter : IRestFilter
    {
        private readonly IOptions<QueryOptions> _options;

        public RestProjectionFilter(IOptions<QueryOptions> options)
        {
            _options = options;
        }

        public QueryOptions Options => _options.Value;

        public void Execute(IDictionary<string, StringValues> qs, ref QueryContext context)
        {
            qs.TryGetValue(_options.Value.ProjectionOperator, out var projections);

            ProjectionOptions options;
            if (projections.Count == 0)
            {
                options = ProjectionOptions.Empty;
            }
            else
            {
                options = new ProjectionOptions();
                foreach (var projection in projections)
                foreach (var value in projection.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var v = value.Trim();
                    var attribute = new Projection
                    {
                        Field = v.ToUpperInvariant()
                    };
                    options.Fields.Add(attribute);
                }
            }

            if (!options.Validate(context.Type, _options.Value, out var errors))
            {
                context.Errors.Add(new Error(ErrorEvents.ValidationFailed, ErrorStrings.ValidationFailed,
                    HttpStatusCode.BadRequest, errors));
            }
            else
            {
                if (options.Fields.Count > 0)
                {
                    var members = TypeAccessor.Create(context.Type).GetMembers();

                    foreach (var field in options.Fields)
                    foreach (var member in members)
                    {
                        if (!field.Field.Equals(member.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (typeof(IEnumerable).IsAssignableFrom(member.Type))
                        {
                            field.Type = ProjectionType.OneToMany;
                            break;
                        }

                        if (member.Type.IsClass && member.Type != typeof(string))
                        {
                            field.Type = ProjectionType.OneToOne;
                            break;
                        }

                        field.Type = ProjectionType.Scalar;
                        break;
                    }
                }

                context.Projections = options;
            }
        }
    }
}

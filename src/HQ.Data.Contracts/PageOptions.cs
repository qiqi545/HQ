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
using HQ.Strings;

namespace HQ.Data.Contracts
{
    public class PageOptions : IQueryValidator
    {
        public static readonly PageOptions Empty = new PageOptions();

        public int Page { get; set; }
        public int PerPage { get; set; }

        public bool Validate(Type type, QueryOptions options, out IList<Error> errors)
        {
            var list = new List<Error>();
            if (Page < 1)
                list.Add(new Error(ErrorEvents.InvalidPageParameter, ErrorStrings.Rosetta_PageRangeInvalid,
                    HttpStatusCode.BadRequest));
            if (PerPage > options.PerPageMax)
                list.Add(new Error(ErrorEvents.InvalidPageParameter, ErrorStrings.Rosetta_PerPageTooHigh,
                    HttpStatusCode.RequestEntityTooLarge));

            errors = list;
            return list.Count == 0;
        }
    }
}

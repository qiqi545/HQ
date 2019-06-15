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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HQ.Data.Contracts.AspNetCore.Mvc
{
    public interface IObjectGetController : IObjectController, IActionFilter, IAsyncActionFilter
    {
        Task<IActionResult> GetAsync(SortOptions sort, PageOptions page, StreamOptions stream, FieldOptions fields,
            FilterOptions filter, ProjectionOptions projection, [FromQuery] IEnumerable<long> ids = null,
            [FromQuery] string query = null, [FromQuery] long startingAt = 0, [FromQuery] int? count = null);

        Task<IActionResult> GetAsync([FromRoute] long id, FieldOptions fields, ProjectionOptions projections);
    }
}

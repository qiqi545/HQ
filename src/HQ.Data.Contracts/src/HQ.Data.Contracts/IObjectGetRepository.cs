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
using System.Threading.Tasks;

namespace HQ.Data.Contracts
{
    public interface IObjectGetRepository
    {
        Task<Operation<IPage<IObject>>> GetAsync(Type type, string query = null, SortOptions sort = null, PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);

        Task<Operation<IObject>> GetAsync(Type type, long id, FieldOptions fields = null, ProjectionOptions projection = null);

        Task<Operation<IStream<IObject>>> GetAsync(IEnumerable<long> ids = null, long startingAt = 0, int? count = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);
    }

    public interface IObjectGetRepository<T> where T : IObject
    {
        Task<Operation<IPage<T>>> GetAsync(string query = null, SortOptions sort = null, PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);

        Task<Operation<T>> GetAsync(long id, FieldOptions fields = null, ProjectionOptions projection = null);

        Task<Operation<IStream<T>>> GetAsync(IEnumerable<long> ids = null, long startingAt = 0, int? count = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);
    }
}

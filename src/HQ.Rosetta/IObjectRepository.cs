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
using System.Dynamic;
using System.Threading.Tasks;

namespace HQ.Rosetta
{
    public interface IObjectRepository
    {
        Task<Operation<IPage<IObject>>> GetAsync(Type type, SortOptions sort = null, PageOptions page = null,
            FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);

        Task<Operation<IObject>> GetAsync(Type type, long id, FieldOptions fields = null,
            ProjectionOptions projection = null);

        Task<Operation<IPage<IObject>>> GetAsync(Type type, string query, SortOptions sort = null,
            PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null,
            ProjectionOptions projection = null);

        Task<Operation<ObjectSave>> SaveAsync(Type type, IObject @object);
        Task<Operation<ObjectSave>> SaveAsync(Type type, IObject @object, DynamicObject partial);

        Task<Operation<IEnumerable<ObjectSave>>> SaveAsync(Type type, IEnumerable<IObject> objects, int startingAt = 0,
            int? count = null);

        Task<Operation<ObjectDelete>> DeleteAsync(Type type, long id);
        Task<Operation<ObjectDelete>> DeleteAsync(Type type, IObject @object);

        Task<Operation<IEnumerable<ObjectSave>>> DeleteAsync(Type type, IEnumerable<long> ids, int startingAt = 0,
            int? count = null);

        Task<Operation<IEnumerable<ObjectSave>>> DeleteAsync(Type type, IEnumerable<IObject> objects,
            int startingAt = 0, int? count = null);
    }
}

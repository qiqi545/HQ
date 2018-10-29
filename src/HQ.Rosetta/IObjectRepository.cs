// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

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

		Task<Operation<IPage<IObject>>> SearchAsync(Type type, string query, SortOptions sort = null,
			PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null,
			ProjectionOptions projection = null);

		Task<Operation<ObjectSave>> SaveAsync(Type type, IObject @object);
		Task<Operation<ObjectSave>> SaveAsync(Type type, IObject @object, DynamicObject partial);
		Task<Operation<IEnumerable<ObjectSave>>> SaveAsync(Type type, IEnumerable<IObject> objects, int count);

		Task<Operation<ObjectDelete>> DeleteAsync(Type type, long id);
		Task<Operation<ObjectDelete>> DeleteAsync(Type type, IObject @object);
		Task<Operation<IEnumerable<ObjectSave>>> DeleteAsync(Type type, IEnumerable<long> ids, int count);
		Task<Operation<IEnumerable<ObjectSave>>> DeleteAsync(Type type, IEnumerable<IObject> objects, int count);
	}
}
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
using System.Threading.Tasks;
using ActiveErrors;
using ActiveStorage;

namespace HQ.Data.Contracts
{
	public interface IObjectGetRepository<in TKey> where TKey : IEquatable<TKey>
	{
		Task<Operation<IPage<IObject>>> GetAsync(Type type, string query = null, SortOptions sort = null,
			PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null,
			ProjectionOptions projection = null);

		Task<Operation<IObject>> GetAsync(Type type, TKey id, FieldOptions fields = null,
			ProjectionOptions projection = null);

		Task<Operation<IStream<IObject>>> GetAsync(Type type, SegmentOptions buffer = null, FieldOptions fields = null,
			FilterOptions filter = null,
			ProjectionOptions projection = null);
	}

	public interface IObjectGetRepository<TObject, in TKey> where TObject : IObject<TKey> where TKey : IEquatable<TKey>
	{
		Task<Operation<IPage<TObject>>> GetAsync(string query = null, SortOptions sort = null, PageOptions page = null,
			FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null);

		Task<Operation<TObject>> GetAsync(TKey id, FieldOptions fields = null, ProjectionOptions projection = null);

		Task<Operation<IStream<TObject>>> GetAsync(SegmentOptions segment, FieldOptions fields = null,
			FilterOptions filter = null, ProjectionOptions projection = null);
	}
}
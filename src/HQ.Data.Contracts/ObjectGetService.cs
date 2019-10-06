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
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HQ.Data.Contracts
{
	/// <summary>
	///     An implementation of <see cref="IObjectGetService{T}" /> that defers access to an
	///     <see cref="IObjectGetService{T}" />.
	/// </summary>
	public class ObjectGetService<TObject> : IObjectGetService<TObject, long> where TObject : IObject
	{
		private readonly IObjectGetRepository<TObject> _repository;

		public ObjectGetService(IObjectGetRepository<TObject> repository) => _repository = repository;

		public virtual async Task<IPage<TObject>> GetAsync(string query = null, SortOptions sort = null,
			PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null,
			ProjectionOptions projection = null)
		{
			var operation = await _repository.GetAsync(query, sort, page, fields, filter, projection);
			return operation.Data;
		}

		public virtual async Task<TObject> GetAsync(long id, FieldOptions fields = null,
			ProjectionOptions projection = null)
		{
			var operation = await _repository.GetAsync(id, fields, projection);
			return operation.Data;
		}

		public virtual async Task<IStream<TObject>> GetAsync(SegmentOptions segment = null, FieldOptions fields = null,
			FilterOptions filter = null,
			ProjectionOptions projection = null)
		{
			var operation = await _repository.GetAsync(segment, fields, filter, projection);
			return operation.Data;
		}
	}

	/// <summary>
	///     An implementation of <see cref="IObjectGetService" /> that defers access to an <see cref="IObjectGetRepository" />.
	/// </summary>
	public class ObjectGetService : IObjectGetService<long>
	{
		private readonly IObjectGetRepository _repository;

		public ObjectGetService(IObjectGetRepository repository) => _repository = repository;

		public virtual async Task<IPage<IObject>> GetAsync(Type type, string query = null, SortOptions sort = null,
			PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null,
			ProjectionOptions projection = null)
		{
			var operation = await _repository.GetAsync(type, query, sort, page, fields, filter, projection);
			return operation.Data;
		}

		public virtual async Task<IObject> GetAsync(Type type, long id, FieldOptions fields = null,
			ProjectionOptions projection = null)
		{
			var operation = await _repository.GetAsync(type, id, fields, projection);
			return operation.Data;
		}

		public virtual async Task<IStream<IObject>> GetAsync(Type type, SegmentOptions segment = null,
			FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null)
		{
			var operation = await _repository.GetAsync(type, segment, fields, filter, projection);
			return operation.Data;
		}
	}
}
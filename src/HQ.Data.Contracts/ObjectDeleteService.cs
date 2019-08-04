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
	/// <summary>
	///     An implementation of <see cref="IObjectDeleteService{T}" /> that defers access to an
	///     <see cref="IObjectDeleteRepository{T}" />.
	/// </summary>
	public class ObjectDeleteService<TObject> : IObjectDeleteService<TObject> where TObject : IObject
	{
		private readonly IObjectDeleteRepository<TObject> _repository;

		public ObjectDeleteService(IObjectDeleteRepository<TObject> repository) => _repository = repository;

		public virtual async Task<ObjectDelete> DeleteAsync(long id)
		{
			var operation = await _repository.DeleteAsync(id);
			return operation.Data;
		}

		public virtual async Task<ObjectDelete> DeleteAsync(TObject @object)
		{
			var operation = await _repository.DeleteAsync(@object);
			return operation.Data;
		}

		public virtual async Task DeleteAsync(IEnumerable<long> ids, long startingAt = 0, int? count = null)
		{
			await _repository.DeleteAsync(ids, startingAt, count);
		}

		public virtual async Task DeleteAsync(IEnumerable<TObject> objects, long startingAt = 0, int? count = null)
		{
			await _repository.DeleteAsync(objects, startingAt, count);
		}
	}

	/// <summary>
	///     An implementation of <see cref="IObjectDeleteService" /> that defers access to an
	///     <see cref="IObjectDeleteRepository" />.
	/// </summary>
	public class ObjectDeleteService : IObjectDeleteService
	{
		private readonly IObjectDeleteRepository _repository;

		public ObjectDeleteService(IObjectDeleteRepository repository) => _repository = repository;

		public virtual async Task<ObjectDelete> DeleteAsync(Type type, long id)
		{
			var operation = await _repository.DeleteAsync(type, id);
			return operation.Data;
		}

		public virtual async Task<ObjectDelete> DeleteAsync(Type type, IObject @object)
		{
			var operation = await _repository.DeleteAsync(type, @object);
			return operation.Data;
		}

		public virtual async Task DeleteAsync(Type type, IEnumerable<long> ids, long startingAt = 0, int? count = null)
		{
			await _repository.DeleteAsync(type, ids, startingAt, count);
		}

		public virtual async Task DeleteAsync(Type type, IEnumerable<IObject> objects, long startingAt = 0,
			int? count = null)
		{
			await _repository.DeleteAsync(type, objects, startingAt, count);
		}
	}
}
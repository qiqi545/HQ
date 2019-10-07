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
	///     An implementation of <see cref="IObjectSaveService{T}" /> that defers access to an
	///     <see cref="IObjectSaveRepository{T}" />.
	/// </summary>
	public class ObjectSaveService<TObject> : IObjectSaveService<TObject, long> where TObject : IObject
	{
		private readonly IObjectSaveRepository<TObject, long> _repository;

		public ObjectSaveService(IObjectSaveRepository<TObject, long> repository) => _repository = repository;

		public virtual async Task<ObjectSave> SaveAsync(TObject @object)
		{
			var operation = await _repository.SaveAsync(@object);
			return operation.Data;
		}

		public virtual async Task<ObjectSave> SaveAsync(TObject @object, List<string> fields)
		{
			var operation = await _repository.SaveAsync(@object, fields);
			return operation.Data;
		}

		public virtual async Task SaveAsync(IEnumerable<TObject> objects, BatchSaveStrategy strategy,
			long startingAt = 0, int? count = null)
		{
			await _repository.SaveAsync(objects, strategy, startingAt, count);
		}
	}

	/// <summary>
	///     An implementation of <see cref="IObjectSaveService" /> that defers access to an
	///     <see cref="IObjectSaveRepository" />.
	/// </summary>
	public class ObjectSaveService : IObjectSaveService<long>
	{
		private readonly IObjectSaveRepository<long> _repository;

		public ObjectSaveService(IObjectSaveRepository<long> repository) => _repository = repository;

		public virtual async Task<ObjectSave> SaveAsync(Type type, IObject @object)
		{
			var operation = await _repository.SaveAsync(type, @object);
			return operation.Data;
		}

		public virtual async Task<ObjectSave> SaveAsync(Type type, IObject @object, List<string> fields)
		{
			var operation = await _repository.SaveAsync(type, @object, fields);
			return operation.Data;
		}

		public virtual async Task SaveAsync(Type type, IEnumerable<IObject> objects, BatchSaveStrategy strategy,
			long startingAt = 0, int? count = null)
		{
			await _repository.SaveAsync(type, objects, strategy, startingAt, count);
		}
	}
}
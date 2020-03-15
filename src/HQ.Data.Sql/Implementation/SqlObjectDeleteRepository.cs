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
using System.Linq;
using System.Threading.Tasks;
using ActiveErrors;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Runtime;
using HQ.Data.SessionManagement;

namespace HQ.Data.Sql.Implementation
{
	public class SqlObjectDeleteRepository<TObject> : IObjectDeleteRepository<TObject, long> where TObject : IObject
	{
		private readonly IDataConnection _db;
		private readonly IObjectGetRepository<TObject, long> _gets;
		private readonly IObjectSaveRepository<TObject, long> _saves;
		private readonly IServerTimestampService _timestamps;

		public SqlObjectDeleteRepository(IDataConnection<RuntimeBuilder> db, IObjectGetRepository<TObject, long> gets,
			IObjectSaveRepository<TObject, long> saves, IServerTimestampService timestamps)
		{
			_db = db;
			_gets = gets;
			_saves = saves;
			_timestamps = timestamps;
		}

		public virtual async Task<Operation<ObjectDelete>> DeleteAsync(long id)
		{
			_db.SetTypeInfo(typeof(TObject));
			var get = await _gets.GetAsync(id, FieldOptions.Empty, ProjectionOptions.Empty);
			return await DeleteAsync(get.Data);
		}

		public virtual async Task<Operation<ObjectDelete>> DeleteAsync(TObject @object)
		{
			_db.SetTypeInfo(typeof(TObject));
			if (@object == null)
				return Operation.FromResult(ObjectDelete.NotFound);
			if (@object.DeletedAt.HasValue)
				return Operation.FromResult(ObjectDelete.Gone);

			@object.DeletedAt = _timestamps.GetCurrentTime();
			var save = await _saves.SaveAsync(@object);
			var errors = save.Errors;

			switch (save.Data)
			{
				case ObjectSave.Updated:
					return Operation.FromResult(ObjectDelete.Deleted, errors);
				case ObjectSave.NotFound:
					return Operation.FromResult(ObjectDelete.NotFound, errors);
				case ObjectSave.NoChanges:
				case ObjectSave.Created:
					throw new NotSupportedException("Unexpected outcome from delete operation");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public virtual Task<Operation> DeleteAsync(SegmentOptions segment)
		{
			_db.SetTypeInfo(typeof(TObject));
			throw new NotImplementedException("Segmentation not available.");
		}

		public virtual Task<Operation> DeleteAsync(IEnumerable<TObject> objects, long startingAt = 0, int? count = null)
		{
			var ids = objects.Skip((int) startingAt).Take(count.GetValueOrDefault()).Select(x => x.Id);
			return DeleteAsync(new SegmentOptions {Ids = ids, StartingAt = 0, Count = count.GetValueOrDefault()});
		}
	}

	public class SqlObjectDeleteRepository : IObjectDeleteRepository<long>
	{
		private readonly IDataConnection _db;
		private readonly IObjectGetRepository<long> _gets;
		private readonly IObjectSaveRepository<long> _saves;
		private readonly IServerTimestampService _timestamps;

		public SqlObjectDeleteRepository(IDataConnection<RuntimeBuilder> db, IObjectGetRepository<long> gets,
			IObjectSaveRepository<long> saves, IServerTimestampService timestamps)
		{
			_db = db;
			_gets = gets;
			_saves = saves;
			_timestamps = timestamps;
		}

		public virtual async Task<Operation<ObjectDelete>> DeleteAsync(Type type, long id)
		{
			_db.SetTypeInfo(type);
			var get = await _gets.GetAsync(type, id, FieldOptions.Empty, ProjectionOptions.Empty);
			return await DeleteAsync(type, get.Data);
		}

		public virtual async Task<Operation<ObjectDelete>> DeleteAsync(Type type, IObject @object)
		{
			_db.SetTypeInfo(type);
			if (@object == null)
				return Operation.FromResult(ObjectDelete.NotFound);
			if (@object.DeletedAt.HasValue)
				return Operation.FromResult(ObjectDelete.Gone);

			@object.DeletedAt = _timestamps.GetCurrentTime();
			var save = await _saves.SaveAsync(type, @object);
			var errors = save.Errors;

			switch (save.Data)
			{
				case ObjectSave.Updated:
					return Operation.FromResult(ObjectDelete.Deleted, errors);
				case ObjectSave.NotFound:
					return Operation.FromResult(ObjectDelete.NotFound, errors);
				case ObjectSave.NoChanges:
				case ObjectSave.Created:
					throw new NotSupportedException("Unexpected outcome from delete operation");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public virtual Task<Operation<IEnumerable<ObjectDelete>>> DeleteAsync(Type type, SegmentOptions segment)
		{
			_db.SetTypeInfo(type);
			throw new NotImplementedException("Segmentation not available.");
		}

		public virtual Task<Operation<IEnumerable<ObjectDelete>>> DeleteAsync(Type type, IEnumerable<IObject> objects,
			long startingAt = 0, int? count = null)
		{
			var ids = objects.Skip((int) startingAt).Take(count.GetValueOrDefault()).Select(x => x.Id);
			return DeleteAsync(type, new SegmentOptions {Ids = ids, StartingAt = 0, Count = count.GetValueOrDefault()});
		}
	}
}
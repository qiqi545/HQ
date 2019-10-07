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
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using HQ.Common;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Runtime;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Queries;

namespace HQ.Data.Sql.Implementation
{
	public class SqlObjectSaveRepository<TObject, TBatchOptions> : IObjectSaveRepository<TObject, long>
		where TObject : IObject
	{
		private readonly IDataBatchOperation<TBatchOptions> _copy;
		private readonly IDataConnection _db;
		private readonly IDataDescriptor _descriptor = SimpleDataDescriptor.Create<TObject>();
		private readonly IServerTimestampService _timestamps;

		public SqlObjectSaveRepository(IDataConnection<RuntimeBuilder> db, IDataBatchOperation<TBatchOptions> batching,
			IServerTimestampService timestamps)
		{
			_db = db;
			_copy = batching;
			_timestamps = timestamps;
		}

		public async Task<Operation<ObjectSave>> SaveAsync(TObject @object)
		{
			_db.SetTypeInfo(typeof(TObject));
			if (@object.Id != 0)
				return await SaveAsync(@object, null);

			@object.CreatedAt = _timestamps.GetCurrentTime();
			var sql = SqlBuilder.Insert(@object, _descriptor);
			var inserted = await _db.Current.ExecuteAsync(sql.Sql, sql.Parameters);
			Debug.Assert(inserted == 1);
			return Operation.FromResult(ObjectSave.Created);
		}

		public async Task<Operation<ObjectSave>> SaveAsync(TObject @object, List<string> fields)
		{
			_db.SetTypeInfo(typeof(TObject));
			var sql = SqlBuilder.Update(@object, _descriptor, fields);
			var updated = await _db.Current.ExecuteAsync(sql.Sql, sql.Parameters);
			Debug.Assert(updated == 1);
			return Operation.FromResult(ObjectSave.Updated);
		}

		public async Task<Operation> SaveAsync(IEnumerable<TObject> objects, BatchSaveStrategy strategy,
			long startingAt = 0, int? count = null)
		{
			_db.SetTypeInfo(typeof(TObject));
			await _db.Current.CopyAsync(_copy, _descriptor, objects, strategy, startingAt, count);
			return Operation.CompletedWithoutErrors;
		}
	}

	public class SqlObjectSaveRepository<TOptions> : IObjectSaveRepository<long>
	{
		private readonly IDataBatchOperation<TOptions> _copy;
		private readonly IDataConnection _db;
		private readonly IServerTimestampService _timestamps;

		public SqlObjectSaveRepository(IDataConnection<RuntimeBuilder> db, IDataBatchOperation<TOptions> batching,
			IServerTimestampService timestamps)
		{
			_db = db;
			_copy = batching;
			_timestamps = timestamps;
		}

		public async Task<Operation<ObjectSave>> SaveAsync(Type type, IObject @object)
		{
			var descriptor = SimpleDataDescriptor.Create(type);
			_db.SetTypeInfo(type);
			if (@object.Id != 0)
				return await SaveAsync(type, @object, null);

			@object.CreatedAt = _timestamps.GetCurrentTime();
			var sql = SqlBuilder.Insert(@object, descriptor);
			var inserted = await _db.Current.ExecuteAsync(sql.Sql, sql.Parameters);
			Debug.Assert(inserted == 1);
			return Operation.FromResult(ObjectSave.Created);
		}

		public async Task<Operation<ObjectSave>> SaveAsync(Type type, IObject @object, List<string> fields)
		{
			var descriptor = SimpleDataDescriptor.Create(type);
			_db.SetTypeInfo(type);
			var sql = SqlBuilder.Update(@object, descriptor, fields);
			var updated = await _db.Current.ExecuteAsync(sql.Sql, sql.Parameters);
			Debug.Assert(updated == 1);
			return Operation.FromResult(ObjectSave.Updated);
		}

		public async Task<Operation> SaveAsync(Type type, IEnumerable<IObject> objects, BatchSaveStrategy strategy,
			long startingAt = 0, int? count = null)
		{
			var descriptor = SimpleDataDescriptor.Create(type);
			_db.SetTypeInfo(type);
			await _db.Current.CopyAsync(_copy, descriptor, objects, strategy, startingAt, count);
			return Operation.CompletedWithoutErrors;
		}
	}
}
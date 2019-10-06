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

namespace HQ.Data.Contracts
{
	public class ObjectService<TObject> : IObjectService<TObject> where TObject : IObject
	{
		private readonly IObjectDeleteService<TObject> _deletes;
		private readonly IObjectGetService<TObject, long> _gets;
		private readonly IObjectSaveService<TObject> _saves;

		public ObjectService(IObjectGetService<TObject, long> gets, IObjectSaveService<TObject> saves, IObjectDeleteService<TObject> deletes)
		{
			_gets = gets;
			_saves = saves;
			_deletes = deletes;
		}

		#region Reads

		public async Task<IPage<TObject>> GetAsync(string query = null, SortOptions sort = null, PageOptions page = null,
			FieldOptions fields = null,
			FilterOptions filter = null, ProjectionOptions projection = null)
		{
			return await _gets.GetAsync(query, sort, page, fields, filter, projection);
		}

		public async Task<TObject> GetAsync(long id, FieldOptions fields = null, ProjectionOptions projection = null)
		{
			return await _gets.GetAsync(id, fields, projection);
		}

		public async Task<IStream<TObject>> GetAsync(SegmentOptions segment = null, FieldOptions fields = null,
			FilterOptions filter = null,
			ProjectionOptions projection = null)
		{
			return await _gets.GetAsync(segment, fields, filter, projection);
		}

		#endregion

		#region Writes

		public async Task<ObjectSave> SaveAsync(TObject @object)
		{
			return await _saves.SaveAsync(@object);
		}

		public async Task<ObjectSave> SaveAsync(TObject @object, List<string> fields)
		{
			return await _saves.SaveAsync(@object, fields);
		}

		public async Task SaveAsync(IEnumerable<TObject> objects, BatchSaveStrategy strategy, long startingAt = 0,
			int? count = null)
		{
			await _saves.SaveAsync(objects, strategy, startingAt, count);
		}

		#endregion

		#region Deletes

		public async Task<ObjectDelete> DeleteAsync(long id)
		{
			return await _deletes.DeleteAsync(id);
		}

		public async Task<ObjectDelete> DeleteAsync(TObject @object)
		{
			return await _deletes.DeleteAsync(@object);
		}

		public async Task DeleteAsync(SegmentOptions segment)
		{
			await _deletes.DeleteAsync(segment);
		}

		public async Task DeleteAsync(IEnumerable<TObject> objects, long startingAt = 0, int? count = null)
		{
			await _deletes.DeleteAsync(objects, startingAt, count);
		}

		#endregion
	}
}
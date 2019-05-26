/*
	This code was generated by a tool. (c) 2019 HQ.IO Corporation. All rights reserved.
    Generated For: Demo
    Generated On: Sunday, May 26, 2019 5:54:55 PM
*/

using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using HQ.Data.Contracts;


namespace HQ.Template
{
    public partial class PersonService : IPersonService
    {
        private readonly IPersonRepository _repository;

        public PersonService(IPersonRepository repository)
        {
            _repository = repository;
        }

        #region Reads

        public virtual async Task<IPage<Person>> GetAsync(string query = null, SortOptions sort = null, PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null)
        {
            var operation = await _repository.GetAsync(query, sort, page, fields, filter, projection);
             return operation.Data;
        }

        public virtual async Task<Person> GetAsync(long id, FieldOptions fields = null, ProjectionOptions projection = null)
        {
            var operation = await _repository.GetAsync(id, fields, projection);
             return operation.Data;
        }

        public virtual async Task<IStream<Person>> GetAsync(IEnumerable<long> ids = null, long startingAt = 0, int? count = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null)
        {
            var operation = await _repository.GetAsync(ids, startingAt, count, fields, filter, projection);
             return operation.Data;
        }

        #endregion

        #region Writes

        public virtual async Task<ObjectSave> SaveAsync(Person @object)
        {
            var operation = await _repository.SaveAsync(@object);
             return operation.Data;
        }

        public virtual async Task<ObjectSave> SaveAsync(Person @object, List<string> fields)
        {
            var operation = await _repository.SaveAsync(@object, fields);
             return operation.Data;
        }

        public virtual async Task SaveAsync(IEnumerable<Person> objects, BatchSaveStrategy strategy, long startingAt = 0, int? count = null)
        {
            await _repository.SaveAsync(objects, strategy, startingAt, count);
        }

        #endregion

        #region Deletes

        public virtual async Task<ObjectDelete> DeleteAsync(long id)
        {
            var operation = await _repository.DeleteAsync(id);
             return operation.Data;
        }

        public virtual async Task<ObjectDelete> DeleteAsync(Person @object)
        {
            var operation = await _repository.DeleteAsync(@object);
             return operation.Data;
        }

        public virtual async Task DeleteAsync(IEnumerable<long> ids, long startingAt = 0, int? count = null)
        {
            await _repository.DeleteAsync(ids, startingAt, count);
        }

        public virtual async Task DeleteAsync(IEnumerable<Person> objects, long startingAt = 0, int? count = null)
        {
            await _repository.DeleteAsync(objects, startingAt, count);
        }

        #endregion
    }
}


/*
	This code was generated by a tool. (c) 2019 HQ.IO Corporation. All rights reserved.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Dapper;
using Morcatko.AspNetCore.JsonMergePatch;
using ErrorStrings = HQ.Data.Contracts.ErrorStrings;
using HQ.Common.FastMember;
using HQ.Common.Models;
using HQ.Common;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries.Rosetta;
using HQ.Data.Sql.Queries;
using HQ.Data.Streaming.Fields;
using HQ.Data.Streaming;
using HQ.DotLiquid;
using HQ.Extensions.CodeGeneration.Scripting;
using HQ.Extensions.Metrics;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Conventions;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Models;
using HQ.Platform.Runtime.Rest.Attributes;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;

namespace HQ.Template
{
    public partial class PersonRepository<TOptions> : IPersonRepository
    {
        private readonly IDataConnection _db;
        private readonly ISqlDialect _dialect;
        private readonly IDataBatchOperation<TOptions> _copy;
        private readonly IServerTimestampService _timestamps;
        private readonly IDataDescriptor _descriptor = SimpleDataDescriptor.Create<Person>();
        private readonly Lazy<FieldOptions> _idField = new Lazy<FieldOptions>(() =>
        {
            var fields = new FieldOptions();
            fields.Fields.Add(nameof(IObject.Id));
            return fields;
        });

        protected SortOptions DefaultSort = SortOptions.FromType<Person>(x => x.Id);
        protected FilterOptions DefaultFilter = FilterOptions.FromType<Person>(x => x.Id);

        public PersonRepository(IDataConnection db, ISqlDialect dialect, IDataBatchOperation<TOptions> batching, IServerTimestampService timestamps)
        {
            _db = db;
            _dialect = dialect;
            _copy = batching;
            _timestamps = timestamps;

        }

        #region Reads

        public virtual async Task<Operation<IPage<Person>>> GetAsync(string query = null, SortOptions sort = null, PageOptions page = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null)
        {
            _db.SetTypeInfo(typeof(Person));
            if (sort == SortOptions.Empty)
                sort = DefaultSort;

                // SELECT * FROM X WHERE Y ORDER BY Z...
                var sql = _dialect.Build<Person>(sort, fields, filter, projection);

                // FETCH OFFSET...
                var data = await _db.Current.QueryAsync<Person>(PageBuilder.Page(_dialect, sql), new { page.Page, page.PerPage });

                // SELECT COUNT(1) WHERE ...
                var total = await _db.Current.ExecuteScalarAsync<long>(_dialect.Count<Person>(_descriptor, filter));

                var slice = new Page<Person>(data, data.Count(), page.Page - 1, page.PerPage, total);

                return new Operation<IPage<Person>>(slice);
        }

        public virtual async Task<Operation<Person>> GetAsync(long id, FieldOptions fields = null, ProjectionOptions projection = null)
        {
            _db.SetTypeInfo(typeof(Person));

             var getById = DefaultFilter;
             getById.Fields[0].Value = id;

            var sql = _dialect.Build<Person>(fields: fields, filter: DefaultFilter, projections: projection);
            var data = await _db.Current.QuerySingleOrDefaultAsync<Person>(sql, new { id });
            return new Operation<Person>(data);
        }


        public virtual Task<Operation<IStream<Person>>> GetAsync(IEnumerable<long> ids, long startingAt = 0, int? count = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null)
        {
            _db.SetTypeInfo(typeof(Person));
            throw new NotImplementedException("Streaming not available.");
        }

        #endregion

        #region Writes

        public virtual async Task<Operation<ObjectSave>> SaveAsync(Person @object)
        {
            _db.SetTypeInfo(typeof(Person));
            if (@object.Id != 0)
                return await SaveAsync(@object, null);

            @object.CreatedAt = _timestamps.GetCurrentTime();
            var sql = SqlBuilder.Insert(@object, _descriptor);
            var inserted = await _db.Current.ExecuteAsync(sql.Sql, sql.Parameters);
            Debug.Assert(inserted == 1);
            return Operation.FromResult(ObjectSave.Created);
        }

        public virtual async Task<Operation<ObjectSave>> SaveAsync(Person @object, List<string> fields)
        {
            _db.SetTypeInfo(typeof(Person));
            var sql = SqlBuilder.Update(@object, _descriptor, fields);
            var updated = await _db.Current.ExecuteAsync(sql.Sql, sql.Parameters);
            Debug.Assert(updated == 1);
            return Operation.FromResult(ObjectSave.Updated);
        }

        public virtual async Task<Operation> SaveAsync(IEnumerable<Person> objects, BatchSaveStrategy strategy, long startingAt = 0, int? count = null)
        {
            _db.SetTypeInfo(typeof(Person));
            await _db.Current.CopyAsync(_copy, _descriptor, objects, strategy, startingAt, count);
            return Operation.CompletedWithoutErrors;
        }

        #endregion

        #region Deletes


        public virtual async Task<Operation<ObjectDelete>> DeleteAsync(long id)
        {
            _db.SetTypeInfo(typeof(Person));
            var get = await GetAsync(id, FieldOptions.Empty, ProjectionOptions.Empty);
            return await DeleteAsync(get.Data);
        }

        public virtual async Task<Operation<ObjectDelete>> DeleteAsync(Person @object)
        {
            _db.SetTypeInfo(typeof(Person));
            if (@object == null)
                return Operation.FromResult(ObjectDelete.NotFound);
            if (@object.DeletedAt.HasValue)
                return Operation.FromResult(ObjectDelete.Gone);

            @object.DeletedAt = _timestamps.GetCurrentTime();
            var save = await SaveAsync(@object);
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

        public virtual Task<Operation> DeleteAsync(IEnumerable<long> ids, long startingAt = 0, int? count = null)
        {
            _db.SetTypeInfo(typeof(Person));
            throw new NotImplementedException("Streaming not available.");
        }

        public virtual Task<Operation> DeleteAsync(IEnumerable<Person> objects, long startingAt = 0, int? count = null)
        {
            _db.SetTypeInfo(typeof(Person));
            throw new NotImplementedException("Streaming not available.");
        }

        #endregion
    }
}


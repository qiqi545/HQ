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
using Dapper;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Configuration;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries;
using Microsoft.Extensions.Options;

namespace HQ.Data.Sql.Implementation
{
	public class SqlObjectGetRepository<TObject> : IObjectGetRepository<TObject> where TObject : IObject
    {
        private readonly IDataConnection _db;
        private readonly ISqlDialect _dialect;
        private readonly IOptionsMonitor<QueryOptions> _options;
        private readonly IDataDescriptor _descriptor = SimpleDataDescriptor.Create<TObject>();

        public SqlObjectGetRepository(IDataConnection db, ISqlDialect dialect, IOptionsMonitor<QueryOptions> options)
        {
            _db = db;
            _dialect = dialect;
            _options = options;
        }

        public async Task<Operation<IPage<TObject>>> GetAsync(string query = null, SortOptions sort = null, PageOptions page = null, FieldOptions fields = null,
            FilterOptions filter = null, ProjectionOptions projection = null)
        {
            _db.SetTypeInfo(typeof(TObject));

            if (sort == null || sort == SortOptions.Empty)
                sort = SortOptions.FromType<TObject>(x => x.Id);

            if (page == null || page == PageOptions.Empty)
                page = new PageOptions { Page = 1, PerPage = _options.CurrentValue.PerPageDefault };

            var sql = _dialect.Build<TObject>(sort, fields, filter, projection);
            var data = (await _db.Current.QueryAsync<TObject>(PageBuilder.Page(_dialect, sql), new { page.Page, page.PerPage })).AsList();
            var total = await _db.Current.ExecuteScalarAsync<long>(_dialect.Count(_descriptor, filter?.Fields));
            var slice = new Page<TObject>(data, data.Count, page.Page - 1, page.PerPage, total);

            return new Operation<IPage<TObject>>(slice);
        }

        public async Task<Operation<TObject>> GetAsync(long id, FieldOptions fields = null, ProjectionOptions projection = null)
        {
            _db.SetTypeInfo(typeof(TObject));

            var getById = FilterOptions.FromType<TObject>(x => x.Id);
            getById.Fields[0].Value = id;

            var sql = _dialect.Build<TObject>(fields: fields, filter: getById, projections: projection);
            var data = await _db.Current.QuerySingleOrDefaultAsync<TObject>(sql, new { id });
            return new Operation<TObject>(data);
        }

        public Task<Operation<IStream<TObject>>> GetAsync(IEnumerable<long> ids = null, long startingAt = 0, int? count = null, FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null)
        {
            _db.SetTypeInfo(typeof(TObject));
            throw new NotImplementedException("Streaming not available.");
        }
    }

    public class SqlObjectGetRepository : IObjectGetRepository
    {
        private readonly IDataConnection _db;
        private readonly ISqlDialect _dialect;
        private readonly IOptionsMonitor<QueryOptions> _options;

        public SqlObjectGetRepository(IDataConnection db, ISqlDialect dialect, IOptionsMonitor<QueryOptions> options)
        {
            _db = db;
            _dialect = dialect;
            _options = options;
        }

        public async Task<Operation<IPage<IObject>>> GetAsync(Type type, string query = null, SortOptions sort = null, PageOptions page = null,
            FieldOptions fields = null, FilterOptions filter = null, ProjectionOptions projection = null)
        {
            var descriptor = SimpleDataDescriptor.Create(type);
            _db.SetTypeInfo(type);

            if (sort == null || sort == SortOptions.Empty)
                sort = GetDefaultSortOptions(descriptor);

            if (page == null || page == PageOptions.Empty)
                page = GetDefaultPageOptions();

            var sql = _dialect.Build(type, sort, fields, filter, projection);
            var data = (await _db.Current.QueryAsync<IObject>(PageBuilder.Page(_dialect, sql), new { page.Page, page.PerPage })).AsList();
            var total = await _db.Current.ExecuteScalarAsync<long>(_dialect.Count(descriptor, filter?.Fields));
            var slice = new Page<IObject>(data, data.Count, page.Page - 1, page.PerPage, total);

            return new Operation<IPage<IObject>>(slice);
        }

        public async Task<Operation<IObject>> GetAsync(Type type, long id, FieldOptions fields = null, ProjectionOptions projection = null)
        {
            var descriptor = SimpleDataDescriptor.Create(type);
            _db.SetTypeInfo(type);

            var getById = GetDefaultFilterOptions(id, descriptor);
            getById.Fields[0].Value = id;

            var sql = _dialect.Build(type, fields: fields, filter: getById, projections: projection);
            var data = await _db.Current.QuerySingleOrDefaultAsync<IObject>(sql, new { id });
            return new Operation<IObject>(data);
        }

        public Task<Operation<IStream<IObject>>> GetAsync(Type type, IEnumerable<long> ids = null, long startingAt = 0, int? count = null, FieldOptions fields = null,
            FilterOptions filter = null, ProjectionOptions projection = null)
        {
            var descriptor = SimpleDataDescriptor.Create(type);
            _db.SetTypeInfo(type);
            throw new NotImplementedException("Streaming not available.");
        }

        private static SortOptions GetDefaultSortOptions(IDataDescriptor descriptor)
        {
            return new SortOptions { Fields = { new Sort { Field = descriptor.Id.ColumnName, Descending = false } } };
        }

        private PageOptions GetDefaultPageOptions()
        {
            return new PageOptions { Page = 1, PerPage = _options.CurrentValue.PerPageDefault };
        }

        private static FilterOptions GetDefaultFilterOptions(long id, IDataDescriptor descriptor)
        {
            var getById = new FilterOptions
            {
                Fields = new List<Filter>
                {
                    new Filter
                    {
                        Field = descriptor.Id.ColumnName,
                        Operator = FilterOperator.Equal,
                        Type = FilterType.Scalar,
                        Value = id
                    }
                }
            };
            return getById;
        }
    }
}

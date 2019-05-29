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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Common.AspNetCore;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Queries;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Stores.Sql
{
    public partial class ApplicationStore<TApplication, TKey> : IQueryableApplicationStore<TApplication>, IApplicationSecurityStampStore<TApplication>
        where TApplication : IdentityApplication<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IDataConnection _connection;
        private readonly IQueryableProvider<TApplication> _queryable;

        public ApplicationStore(
            IDataConnection connection,
            IQueryableProvider<TApplication> queryable,
            IServiceProvider serviceProvider)
        {
            serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
            CancellationToken = cancellationToken;
            _connection = connection;
            _queryable = queryable;
        }

        public CancellationToken CancellationToken { get; }

        public IQueryable<TApplication> Applications => MaybeQueryable();

        public async Task<IdentityResult> CreateAsync(TApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            application.ConcurrencyStamp = application.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            if (application.Id == null)
            {
                var idType = typeof(TKey);
                var id = Guid.NewGuid();
                if (idType == typeof(Guid))
                {
                    application.Id = (TKey)(object)id;
                }
                else if (idType == typeof(string))
                {
                    application.Id = (TKey)(object)$"{id}";
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            var query = SqlBuilder.Insert(application);
            _connection.SetTypeInfo(typeof(TApplication));

            var inserted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            Debug.Assert(inserted == 1);

            if (application.Id == null && _connection.TryGetLastInsertedId(out TKey insertedId))
            {
                application.Id = insertedId;
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            application.ConcurrencyStamp = application.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            var query = SqlBuilder.Update(application, new { application.Id });
            _connection.SetTypeInfo(typeof(TApplication));

            var updated = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            Debug.Assert(updated == 1);
            return IdentityResult.Success;
        }

        public Task SetApplicationNameAsync(TApplication application, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            application.Name = name;
            return Task.CompletedTask;
        }

        public Task SetNormalizedApplicationNameAsync(TApplication application, string normalizedName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            application.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> DeleteAsync(TApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Delete<TApplication>(new { application.Id });
            _connection.SetTypeInfo(typeof(TApplication));
            var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

            Debug.Assert(deleted == 1);
            return IdentityResult.Success;
        }

        public async Task<TApplication> FindByIdAsync(string applicationId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var id = StringToId(applicationId);

            var query = SqlBuilder.Select<TApplication>(new { Id = id });
            _connection.SetTypeInfo(typeof(TApplication));

            var application = await _connection.Current.QuerySingleOrDefaultAsync<TApplication>(query.Sql, query.Parameters);
            return application;
        }

        public async Task<IEnumerable<TApplication>> FindByIdsAsync(IEnumerable<string> applicationIds,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ids = new List<object>();
            foreach (var tenantId in applicationIds)
            {
                ids.Add(StringToId(tenantId));
            }

            var query = SqlBuilder.Select<TApplication>(new { Id = ids });
            _connection.SetTypeInfo(typeof(TApplication));

            var applications = await _connection.Current.QueryAsync<TApplication>(query.Sql, query.Parameters);
            return applications;
        }

        public async Task<TApplication> FindByNameAsync(string normalizedApplicationName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Select<TApplication>(new { NormalizedName = normalizedApplicationName });
            _connection.SetTypeInfo(typeof(TApplication));

            var application = await _connection.Current.QuerySingleOrDefaultAsync<TApplication>(query.Sql, query.Parameters);
            return application;
        }

        public Task<string> GetApplicationIdAsync(TApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(application?.Id?.ToString());
        }

        public Task<string> GetApplicationNameAsync(TApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(application?.Name);
        }

        public void Dispose()
        {
        }

        private static object StringToId(string applicationId)
        {
            var idType = typeof(TKey);
            object id;
            if (idType == typeof(Guid) && Guid.TryParse(applicationId, out var guid))
            {
                id = guid;
            }
            else if ((idType == typeof(short) || idType == typeof(int) || idType == typeof(long)) &&
                     long.TryParse(applicationId, out var integer))
            {
                id = integer;
            }
            else if (idType == typeof(string))
            {
                id = applicationId;
            }
            else
            {
                throw new NotSupportedException();
            }

            return id;
        }

        private IQueryable<TApplication> MaybeQueryable()
        {
            if (_queryable.IsSafe)
            {
                return _queryable.Queryable;
            }

            if (_queryable.SupportsUnsafe)
            {
                return _queryable.UnsafeQueryable;
            }

            return Task.Run(() => GetAllApplicationsAsync(CancellationToken), CancellationToken).Result.AsQueryable();
        }

        private async Task<IEnumerable<TApplication>> GetAllApplicationsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = SqlBuilder.Select<TApplication>();
            _connection.SetTypeInfo(typeof(TApplication));
            var applications = await _connection.Current.QueryAsync<TApplication>(query.Sql, query.Parameters);
            return applications;
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = SqlBuilder.Count<TApplication>();
            _connection.SetTypeInfo(typeof(TApplication));
            var count = await _connection.Current.ExecuteScalarAsync<int>(query.Sql, query.Parameters);
            return count;
        }

        public Task SetSecurityStampAsync(TApplication application, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            application.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(TApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(application?.SecurityStamp);
        }
    }
}

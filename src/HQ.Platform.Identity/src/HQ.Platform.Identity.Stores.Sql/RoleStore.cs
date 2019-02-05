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
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Queries;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Stores.Sql
{
    public partial class RoleStore<TKey, TRole> :
        IRoleStoreExtended<TRole>,
        IQueryableRoleStore<TRole>
        where TRole : IdentityRoleExtended<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IDataConnection _connection;
        private readonly IQueryableProvider<TRole> _queryable;
        private readonly int _tenantId;

        public RoleStore(IDataConnection connection, IQueryableProvider<TRole> queryable, IServiceProvider serviceProvider)
        {
            serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
            serviceProvider.TryGetTenantId(out _tenantId);

            CancellationToken = cancellationToken;

            _connection = connection;
            _queryable = queryable;
        }

        public CancellationToken CancellationToken { get; }

        public IQueryable<TRole> Roles => MaybeQueryable();

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            role.TenantId = _tenantId;
            role.ConcurrencyStamp = role.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            if (role.Id == null)
            {
                var idType = typeof(TKey);
                var id = Guid.NewGuid();
                if (idType == typeof(Guid))
                    role.Id = (TKey) (object) id;
                else if (idType == typeof(string))
                    role.Id = (TKey) (object) $"{id}";
                else
                    throw new NotSupportedException();
            }

            var query = SqlBuilder.Insert(role);
            _connection.SetTypeInfo(typeof(TRole));
            var inserted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

            Debug.Assert(inserted == 1); 
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            role.ConcurrencyStamp = role.ConcurrencyStamp ?? $"{Guid.NewGuid()}";
            
            var query = SqlBuilder.Update(role, new { role.Id, TenantId = _tenantId });
            _connection.SetTypeInfo(typeof(TRole));
            var updated = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

            Debug.Assert(updated == 1);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            role.ConcurrencyStamp = role.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            var query = SqlBuilder.Delete<TRole>(new { role.Id, TenantId = _tenantId });
            _connection.SetTypeInfo(typeof(TRole));
            var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

            Debug.Assert(deleted == 1);
            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(role?.Id?.ToString());
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(role?.Name);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(role?.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Select<TRole>(new { Id = roleId, TenantId = _tenantId });
            _connection.SetTypeInfo(typeof(TRole));

            var role = await _connection.Current.QuerySingleOrDefaultAsync<TRole>(query.Sql, query.Parameters);
            return role;
        }

        public async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Select<TRole>(new {NormalizedName = normalizedRoleName, TenantId = _tenantId });
            _connection.SetTypeInfo(typeof(TRole));

            var role = await _connection.Current.QuerySingleOrDefaultAsync<TRole>(query.Sql, query.Parameters);
            return role;
        }

        private IQueryable<TRole> MaybeQueryable()
        {
            if (_queryable.IsSafe)
                return _queryable.Queryable;

            if (_queryable.SupportsUnsafe)
                return _queryable.UnsafeQueryable;

            return Task.Run(GetAllRolesAsync, CancellationToken).Result.AsQueryable();
        }

        private async Task<IEnumerable<TRole>> GetAllRolesAsync()
        {
            var query = SqlBuilder.Select<TRole>(new { TenantId = _tenantId });
            _connection.SetTypeInfo(typeof(TRole));
            var roles = await _connection.Current.QueryAsync<TRole>(query.Sql, query.Parameters);
            return roles;
        }

        public void Dispose() { }}
}

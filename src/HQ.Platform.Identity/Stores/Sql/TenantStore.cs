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
	public class TenantStore<TTenant, TKey> : 
		IQueryableTenantStore<TTenant>, 
		ITenantSecurityStampStore<TTenant>
		where TTenant : IdentityTenant<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IDataConnection _connection;
		private readonly IQueryableProvider<TTenant> _queryable;

		public TenantStore(
			IDataConnection<IdentityBuilder> connection,
			IQueryableProvider<TTenant> queryable,
			IServiceProvider serviceProvider)
		{
			serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
			CancellationToken = cancellationToken;
			_connection = connection;
			_queryable = queryable;
		}

		public CancellationToken CancellationToken { get; }

		public IQueryable<TTenant> Tenants => MaybeQueryable();

		public async Task<IdentityResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			tenant.ConcurrencyStamp = tenant.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

			if (tenant.Id == null)
			{
				var idType = typeof(TKey);
				var id = Guid.NewGuid();
				if (idType == typeof(Guid))
				{
					tenant.Id = (TKey) (object) id;
				}
				else if (idType == typeof(string))
				{
					tenant.Id = (TKey) (object) $"{id}";
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			var query = SqlBuilder.Insert(tenant);
			_connection.SetTypeInfo(typeof(TTenant));

			var inserted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
			Debug.Assert(inserted == 1);

			if (tenant.Id == null && _connection.TryGetLastInsertedId(out TKey insertedId))
				tenant.Id = insertedId;

			return IdentityResult.Success;
		}

		public async Task<IdentityResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			tenant.ConcurrencyStamp = tenant.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

			var query = SqlBuilder.Update(tenant, new {tenant.Id});
			_connection.SetTypeInfo(typeof(TTenant));

			var updated = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
			Debug.Assert(updated == 1);
			return IdentityResult.Success;
		}

		public Task SetTenantNameAsync(TTenant tenant, string name, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			tenant.Name = name;
			return Task.CompletedTask;
		}

		public Task SetNormalizedTenantNameAsync(TTenant tenant, string normalizedName,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			tenant.NormalizedName = normalizedName;
			return Task.CompletedTask;
		}

		public async Task<IdentityResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var query = SqlBuilder.Delete<TTenant>(new {tenant.Id});
			_connection.SetTypeInfo(typeof(TTenant));
			var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

			Debug.Assert(deleted == 1);
			return IdentityResult.Success;
		}

		public async Task<TTenant> FindByIdAsync(string tenantId, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var id = StringToId(tenantId);

			var query = SqlBuilder.Select<TTenant>(new {Id = id});
			_connection.SetTypeInfo(typeof(TTenant));

			var tenant = await _connection.Current.QuerySingleOrDefaultAsync<TTenant>(query.Sql, query.Parameters);
			return tenant;
		}

		public async Task<IEnumerable<TTenant>> FindByIdsAsync(IEnumerable<string> tenantIds,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var ids = new List<object>();
			foreach (var tenantId in tenantIds)
			{
				ids.Add(StringToId(tenantId));
			}

			var query = SqlBuilder.Select<TTenant>(new {Id = ids});
			_connection.SetTypeInfo(typeof(TTenant));

			var tenants = await _connection.Current.QueryAsync<TTenant>(query.Sql, query.Parameters);
			return tenants;
		}

		public async Task<TTenant> FindByNameAsync(string normalizedTenantName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var query = SqlBuilder.Select<TTenant>(new {NormalizedName = normalizedTenantName});
			_connection.SetTypeInfo(typeof(TTenant));

			var tenant = await _connection.Current.QuerySingleOrDefaultAsync<TTenant>(query.Sql, query.Parameters);
			return tenant;
		}

		public Task<string> GetTenantIdAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(tenant?.Id?.ToString());
		}

		public Task<string> GetTenantNameAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(tenant?.Name);
		}

		public void Dispose()
		{
		}

		public async Task<int> GetCountAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var query = SqlBuilder.Count<TTenant>();
			_connection.SetTypeInfo(typeof(TTenant));
			var count = await _connection.Current.ExecuteScalarAsync<int>(query.Sql, query.Parameters);
			return count;
		}

		public Task SetSecurityStampAsync(TTenant tenant, string stamp, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			tenant.SecurityStamp = stamp;
			return Task.CompletedTask;
		}

		public Task<string> GetSecurityStampAsync(TTenant tenant, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(tenant?.SecurityStamp);
		}

		private static object StringToId(string tenantId)
		{
			var idType = typeof(TKey);
			object id;
			if (idType == typeof(Guid) && Guid.TryParse(tenantId, out var guid))
			{
				id = guid;
			}
			else if ((idType == typeof(short) || idType == typeof(int) || idType == typeof(long)) &&
			         long.TryParse(tenantId, out var integer))
			{
				id = integer;
			}
			else if (idType == typeof(string))
			{
				id = tenantId;
			}
			else
			{
				throw new NotSupportedException();
			}

			return id;
		}

		private IQueryable<TTenant> MaybeQueryable()
		{
			if (_queryable.IsSafe)
			{
				return _queryable.Queryable;
			}

			if (_queryable.SupportsUnsafe)
			{
				return _queryable.UnsafeQueryable;
			}

			return Task.Run(() => GetAllTenantsAsync(CancellationToken), CancellationToken).Result.AsQueryable();
		}

		private async Task<IEnumerable<TTenant>> GetAllTenantsAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var query = SqlBuilder.Select<TTenant>();
			_connection.SetTypeInfo(typeof(TTenant));
			var tenants = await _connection.Current.QueryAsync<TTenant>(query.Sql, query.Parameters);
			return tenants;
		}
	}
}
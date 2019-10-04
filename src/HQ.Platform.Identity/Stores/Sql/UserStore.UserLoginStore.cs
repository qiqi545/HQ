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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Data.Sql.Queries;
using HQ.Platform.Identity.Stores.Sql.Models;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Stores.Sql
{
	partial class UserStore<TUser, TKey, TRole> : IUserLoginStore<TUser>
	{
		public async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			_connection.SetTypeInfo(typeof(AspNetUserLogins<TKey>));

			var query = SqlBuilder.Insert(new AspNetUserLogins<TKey>
			{
				LoginProvider = login.LoginProvider,
				ProviderKey = login.ProviderKey,
				ProviderDisplayName = login.ProviderDisplayName,
				UserId = user.Id,
				TenantId = _tenantId
			});

			var inserted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

			Debug.Assert(inserted == 1);
		}

		public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			_connection.SetTypeInfo(typeof(AspNetUserLogins<TKey>));

			var query = SqlBuilder.Delete<AspNetUserLogins<TKey>>(new
			{
				UserId = user.Id, LoginProvider = loginProvider, ProviderKey = providerKey, TenantId = _tenantId
			});

			var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
			Debug.Assert(deleted == 1);
		}

		public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			_connection.SetTypeInfo(typeof(AspNetUserLogins<TKey>));

			var query = SqlBuilder.Select<AspNetUserLogins<TKey>>(new {UserId = user.Id, TenantId = _tenantId});

			var logins = await _connection.Current.QueryAsync<AspNetUserLogins<TKey>>(query.Sql, query.Parameters);

			return logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))
				.AsList();
		}

		public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			_connection.SetTypeInfo(typeof(AspNetUserLogins<TKey>));

			var query = SqlBuilder.Select<AspNetUserLogins<TKey>>(new
			{
				LoginProvider = loginProvider, ProviderKey = providerKey, TenantId = _tenantId
			});

			var user = await _connection.Current.QuerySingleOrDefaultAsync<AspNetUserLogins<TKey>>(query.Sql,
				query.Parameters);
			if (user == null)
				return null;

			return await FindByIdAsync(user.UserId.ToString(), CancellationToken);
		}
	}
}